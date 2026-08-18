using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Core.Aspects.Autofac.Logging;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.Transaction;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace Business.Concrete
{
    public class ReservationManager : IReservationService
    {
        private readonly IReservationDal _reservationDal;
        private readonly IRedisLockService _redisLockService;
        private readonly IFieldPriceSheduleDal _fieldPriceScheduleDal;
        private readonly IFieldDal _footballFieldDal;
        private readonly ITimeSlotDal _timeSlotDal;
        private readonly IUserDal _userDal;
        private readonly IFreeBookingRightDal _freeBookingRightDal;
        private readonly IReservationNotificationService _notificationService;

        private static readonly object _reservationLock = new object();


        public ReservationManager(
            IReservationDal reservationDal,
            IFieldPriceSheduleDal fieldPriceScheduleDal,
            IFieldDal footballFieldDal,
            ITimeSlotDal timeSlotDal,
            IUserDal userDal,
            IRedisLockService redisLockService,
            IReservationNotificationService notificationService,
            IFreeBookingRightDal freeBookingRightDal)
        {
            _reservationDal = reservationDal;
            _redisLockService = redisLockService;
            _fieldPriceScheduleDal = fieldPriceScheduleDal;
            _footballFieldDal = footballFieldDal;
            _timeSlotDal = timeSlotDal;
            _userDal = userDal;
            _notificationService = notificationService;
            _freeBookingRightDal = freeBookingRightDal;
        }

        public IDataResult<List<FootballFieldScheduleDto>> GetAllWeeklySchedules(int businessId)
        {
            var schedules = _reservationDal.GetAllWeeklySchedulesByBusinessId(businessId);
            return new SuccessDataResult<List<FootballFieldScheduleDto>>(schedules, "İşletmenin haftalık takvim şablonu başarıyla getirildi.");
        }

        // 2. DOLU SLOTLAR (Veritabanından)
        public IDataResult<List<SlotStateDto>> GetBookedSlotsByDateRange(int businessId, DateOnly startDate, DateOnly endDate)
        {
            var bookedSlots = _reservationDal.GetBookedSlotsByDateRange(businessId, startDate, endDate);
            return new SuccessDataResult<List<SlotStateDto>>(bookedSlots, "Dolu slotlar başarıyla getirildi.");
        }

        // 3. İŞLEMDE OLAN SLOTLAR (Redis'ten)
        public async Task<IDataResult<List<SlotStateDto>>> GetHeldSlotsByDateRangeAsync(int businessId, DateOnly startDate, DateOnly endDate)
        {
            var heldSlots = new List<SlotStateDto>();

            var allSchedules = GetAllWeeklySchedules(businessId).Data;
            if (allSchedules == null || allSchedules.Count == 0)
            {
                return new SuccessDataResult<List<SlotStateDto>>(heldSlots);
            }

            
            var scheduleIds = allSchedules
                .SelectMany(f => f.Schedules.Select(s => s.FieldPriceScheduleId))
                .ToList();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var heldIdsForDate = await _redisLockService.GetActiveHoldsAsync(businessId, date, scheduleIds);

                foreach (var id in heldIdsForDate)
                {
                    heldSlots.Add(new SlotStateDto { ScheduleId = id, Date = date });
                }
            }

            return new SuccessDataResult<List<SlotStateDto>>(heldSlots, "İşlemde olan slotlar getirildi.");
        }


        public async Task<IResult> HoldReservationSlotAsync(int businessId, DateOnly date, int scheduleId, int userId)
        {
            bool isLocked = await _redisLockService.LockSlotAsync(businessId, date, scheduleId, userId, 5);

            if (isLocked)
            {
                // 🚀 GÜNCELLENDİ: Artık tarihi de yolluyoruz!
                await _notificationService.SendSlotHeldNotificationAsync(businessId, date, scheduleId);

                return new SuccessResult("Saha 5 dakikalığına sizin için rezerve edildi. Lütfen işlemi tamamlayın.");
            }

            return new ErrorResult("Bu saha şu anda başka bir kullanıcı tarafından işlem görüyor.");
        }



        
        

        

        public async Task<IResult> CancelHoldSlotAsync(int businessId, DateOnly date, int scheduleId, int userId)
        {
            // 1. Kilidin sahibini kontrol et (Sadece kilitleyen kişi iptal edebilir!)
            int? lockOwner = await _redisLockService.GetLockOwnerAsync(businessId, date, scheduleId);

            if (lockOwner.HasValue && lockOwner.Value == userId)
            {
                // 2. Kilidi Redis'ten sil
                await _redisLockService.UnlockSlotAsync(businessId, date, scheduleId);

                // 3. SignalR ile odadaki herkesin ekranında bu slotu tekrar YEŞİL (boş) yap!
                await _notificationService.SendSlotUnlockedNotificationAsync(businessId, date, scheduleId);

                return new SuccessResult("Geçici rezervasyon işlemi iptal edildi.");
            }

            return new ErrorResult("Bu işlem size ait değil veya süresi çoktan dolmuş.");
        }



        [SecuredOperation("user")]
        //[TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public async Task<IResult> CreateReservationAsync(CreateReservationDto createDto, int userId)
        {
            using (var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                var today = DateOnly.FromDateTime(DateTime.Now);

                // 1. Gün geçmiş mi? (Dün veya öncesi mi?)
                if (createDto.ReservationDate < today)
                {
                    return new ErrorResult("Geçmiş tarihlere rezervasyon yapılamaz!");
                }

                // 2. Bugün seçilmişse, saat geçmiş mi?
                if (createDto.ReservationDate == today)
                {
                    var slotStartTime = _reservationDal.GetStartTimeByScheduleId(createDto.FieldPriceScheduleId);
                    var currentTime = TimeOnly.FromDateTime(DateTime.Now);

                    if (slotStartTime <= currentTime)
                    {
                        return new ErrorResult("Geçmiş saatlere rezervasyon yapılamaz!");
                    }
                }

                // 3. SİSTEM İHLALİ (HACKER) KONTROLÜ
                int requestedDayOfWeek = (int)createDto.ReservationDate.DayOfWeek;
                int requestedDbDayId = requestedDayOfWeek == 0 ? 7 : requestedDayOfWeek;

                int actualSlotDayId = _reservationDal.GetDayIdByScheduleId(createDto.FieldPriceScheduleId);

                if (actualSlotDayId != requestedDbDayId)
                {
                    return new ErrorResult("Sistem İhlali Tespit Edildi: Seçilen tarih ile rezerve edilmek istenen saatin günü uyuşmuyor!");
                }

                // 4. VERİTABANI GÜVENLİĞİ: İçeri giren kişinin istediği slot az önce doldurulmuş mu?
                bool isBooked = _reservationDal.IsSlotBooked(createDto.FieldPriceScheduleId, createDto.ReservationDate);

                if (isBooked)
                {
                    return new ErrorResult("Üzgünüz, bu saha ve saat az önce başka biri tarafından rezerve edildi. Lütfen başka bir saat seçiniz.");
                }

                // 5. REDIS KONTROLÜ
                int? lockOwnerId = await _redisLockService.GetLockOwnerAsync(createDto.BusinessId, createDto.ReservationDate, createDto.FieldPriceScheduleId);

                if (lockOwnerId.HasValue && lockOwnerId.Value != userId)
                {
                    return new ErrorResult("Bu saha şu anda başka bir kullanıcı tarafından ödeme aşamasında. Lütfen 5 dakika sonra tekrar deneyin.");
                }

                // 🚀 YENİ EKLENEN KISIM: ÜCRETSİZ HAK KONTROLÜ
                bool usedFreeRight = false;
                decimal finalPrice = createDto.FinalPrice; // DTO'dan gelen normal fiyat

                if (createDto.UseFreeRight)
                {
                    // Veritabanına gidip hakkı 1 azaltmayı deniyoruz
                    usedFreeRight = _reservationDal.TryUseFreeRight(userId, createDto.FieldPriceScheduleId);

                    if (!usedFreeRight)
                    {
                        return new ErrorResult("Ücretsiz değişim hakkınız bulunmamaktadır veya tükenmiştir!");
                    }

                    finalPrice = 0; // Hak kullanıldıysa fiyat sıfırlanır!
                }

                // 6. KONTROLLER BAŞARILI: Güvenle rezervasyonu oluştur
                var reservation = new Entities.Concrete.Reservation
                {
                    FieldPriceScheduleId = createDto.FieldPriceScheduleId,
                    ReservationDate = createDto.ReservationDate,
                    FinalPrice = finalPrice, // 🚀 Ücretsizse 0, değilse normal fiyat kaydedilir
                    StatusId = 1, // 1 = Aktif/Onaylandı
                    UserId = userId,
                    IsUsedFreeRight = usedFreeRight // 🚀 Ücretsiz alındıysa işaretle
                };

                // Veritabanına Ekleme (DAL üzerinden)
                _reservationDal.Add(reservation);

                // 7. TEMİZLİK VE BİLDİRİM
                await _redisLockService.UnlockSlotAsync(createDto.BusinessId, createDto.ReservationDate, createDto.FieldPriceScheduleId);
                await _notificationService.SendSlotBookedNotificationAsync(createDto.BusinessId, createDto.ReservationDate, createDto.FieldPriceScheduleId);

                // 🚀 HER ŞEY YOLUNDA: İşlemi onayla ve veritabanına kalıcı olarak yaz!
                transactionScope.Complete();

                // 🚀 Mesajı dinamikleştiriyoruz ki kullanıcı bedava aldığını hissetsin
                return new SuccessResult(usedFreeRight ? "Ücretsiz değişim hakkınız kullanılarak rezervasyon başarıyla oluşturuldu." : "Rezervasyon başarıyla oluşturuldu.");
            }
        }
        public IDataResult<List<UserReservationDetailDto>> GetUserReservations(int userId)
        {
            var data = _reservationDal.GetUserReservations(userId);
            return new SuccessDataResult<List<UserReservationDetailDto>>(data, "Rezervasyon geçmişiniz başarıyla getirildi.");
        }

        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult CancelReservation(int reservationId, int userId)
        {
            
            var reservation = _reservationDal.Get(r => r.Id == reservationId);

            if (reservation == null)
            {
                return new ErrorResult("Böyle bir rezervasyon bulunamadı.");
            }

            
            if (reservation.UserId != userId)
            {
                return new ErrorResult("Bu rezervasyonu iptal etme yetkiniz bulunmamaktadır.");
            }

            // 3. Durum Kontrolü: Sadece "Onaylandı" (StatusId = 1) olanlar iptal edilebilir
            if (reservation.StatusId != 1)
            {
                return new ErrorResult("Bu rezervasyon zaten iptal edilmiş veya süresi dolmuş.");
            }

            // 4. İptal İşlemi: StatusId'yi 2 (İptal Edildi) olarak güncelle ve kaydet
            reservation.StatusId = 2; // Eğer senin DB'de 3 ise burayı 3 yapabilirsin
            _reservationDal.Update(reservation);

            return new SuccessResult("Rezervasyonunuz başarıyla iptal edildi.");
        }

        public IDataResult<DailyReservationSummaryDto> GetDailyReservations(int businessId, DateTime date)
        {
            var targetDate = DateOnly.FromDateTime(date);

            // Sadece DAL'ı çağırıp sonucu dönüyoruz. SOLID'in Single Responsibility (Tek Sorumluluk) prensibine tam uyum!
            var summaryDto = _reservationDal.GetDailyReservationSummary(businessId, targetDate);

            return new SuccessDataResult<DailyReservationSummaryDto>(summaryDto, "Günlük rezervasyonlar getirildi.");
        }
        // Business -> Concrete -> ReservationManager.cs içine ekle:
        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult CancelReservationByBusiness(int reservationId)
        {
            var reservation = _reservationDal.Get(r => r.Id == reservationId);
            if (reservation == null)
            {
                return new ErrorResult("Rezervasyon bulunamadı.");
            }

            // İptal edilecek saatin StartTime'ını DAL üzerinden çekiyoruz
            var slotStartTime = _reservationDal.GetStartTimeByScheduleId(reservation.FieldPriceScheduleId);

            // Rezervasyonun tam başlama anını oluşturuyoruz (Tarih + Saat)
            DateTime reservationDateTime = reservation.ReservationDate.ToDateTime(slotStartTime);
            // GEÇMİŞ ZAMAN KONTROLÜ!
            if (reservationDateTime <= DateTime.Now)
            {
                return new ErrorResult("Geçmişteki veya şu an oynanmakta olan bir rezervasyonu iptal edemezsiniz.");
            }

            // Her şey yolundaysa iptal et (StatusId = 2 yapıyoruz)
            reservation.StatusId = 2;
            _reservationDal.Update(reservation);

            return new SuccessResult("Rezervasyon işletme tarafından başarıyla iptal edildi. Kullanıcı paneline iade bilgisi yansıtıldı.");
        }
        public IDataResult<int> CheckFreeBookingRights(int userId, int footballFieldId)
        {
            // DAL'dan hakkı çekiyoruz (Hiç yoksa 0 gelecek)
            var count = _reservationDal.GetFreeRightCount(userId, footballFieldId);

            return new SuccessDataResult<int>(count, "Kullanıcının ücretsiz hak sayısı başarıyla getirildi.");
        }

        [TransactionScopeAspect] // Hata olursa iki tablo da geri alınsın
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public IResult UseFreeBookingRight(int reservationId)
        {
            // 1. İlgili hakkı ReservationId üzerinden bul
            var freeBookingRight = _freeBookingRightDal.Get(f => f.ReservationId == reservationId);

            // Hak yoksa veya çoktan kullanıldıysa işlemi reddet
            if (freeBookingRight == null || freeBookingRight.IsUsed)
            {
                return new ErrorResult("Kullanılabilir bir ücretsiz değişim hakkı bulunamadı veya bu hak zaten kullanılmış.");
            }

            // 2. İlgili rezervasyonu bul
            var reservation = _reservationDal.Get(r => r.Id == reservationId);
            if (reservation == null)
            {
                return new ErrorResult("Böyle bir rezervasyon bulunamadı.");
            }

            // 3. Hak tablosunu "Kullanıldı" (True) olarak güncelle
            freeBookingRight.IsUsed = true;
            _freeBookingRightDal.Update(freeBookingRight);

            // 4. Rezervasyon tablosunu "Ücretsiz Değişim Kullanıldı" (StatusId = 5) olarak güncelle
            reservation.StatusId = 5;
            _reservationDal.Update(reservation);

            return new SuccessResult("Ücretsiz değişim hakkı başarıyla kullanıldı ve kart pasife alındı.");
        }

    }
}

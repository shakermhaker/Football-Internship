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
        private readonly IReservationNotificationService _notificationService;

        private static readonly object _reservationLock = new object();


        public ReservationManager(
            IReservationDal reservationDal,
            IFieldPriceSheduleDal fieldPriceScheduleDal,
            IFieldDal footballFieldDal,
            ITimeSlotDal timeSlotDal,
            IUserDal userDal,
            IRedisLockService redisLockService,
            IReservationNotificationService notificationService)
        {
            _reservationDal = reservationDal;
            _redisLockService = redisLockService;
            _fieldPriceScheduleDal = fieldPriceScheduleDal;
            _footballFieldDal = footballFieldDal;
            _timeSlotDal = timeSlotDal;
            _userDal = userDal;
            _notificationService = notificationService;

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



        public IDataResult<List<FootballFieldScheduleDto>> GetBusinessFieldSchedules(int businessId, DateOnly date)
        {
            
            int dayOfWeek = (int)date.DayOfWeek;

            
            int dbDayId = dayOfWeek == 0 ? 7 : dayOfWeek;

            // 3. Sadece o günün (örneğin sadece Cuma'nın) slotlarını çekiyoruz
            var data = _reservationDal.GetFieldSchedulesByBusinessId(businessId, dbDayId);
            return new SuccessDataResult<List<FootballFieldScheduleDto>>(data, "Seçilen tarihe ait takvim verisi başarıyla çekildi.");
        }
        public IDataResult<List<int>> GetBookedScheduleIdsByDate(int businessId, DateOnly date)
        {
            var bookedIds = _reservationDal.GetBookedScheduleIdsByDate(businessId, date);
            return new SuccessDataResult<List<int>>(bookedIds, "Dolu slotlar başarıyla getirildi.");
        }

        public async Task<IDataResult<List<int>>> GetHeldScheduleIdsByDateAsync(int businessId, DateOnly date)
        {
            // 1. İşletmeye ait tüm takvimi (Schedule) çek
            var allSchedules = GetBusinessFieldSchedules(businessId, date).Data;
            if (allSchedules == null || allSchedules.Count == 0)
            {
                return new SuccessDataResult<List<int>>(new List<int>());
            }

            // 2. Takvim içindeki tüm ID'leri düz bir listeye çevir (Örn: [15, 16, 17, ...])
            var scheduleIds = allSchedules
                .SelectMany(field => field.Schedules.Select(slot => slot.FieldPriceScheduleId))
                .ToList();

            // 3. Bu ID listesini Redis'e ver ve sadece kilitli olanları ayıkla
            var heldIds = await _redisLockService.GetActiveHoldsAsync(businessId, date, scheduleIds);

            return new SuccessDataResult<List<int>>(heldIds, "İşlemde olan slotlar getirildi.");
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
        [TransactionScopeAspect]
        [LogAspect]
        [ExceptionLogAspect]
        [PerformanceAspect(2)]
        public async Task<IResult> CreateReservationAsync(CreateReservationDto createDto, int userId)
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

            // 5. 🚀 YENİ - REDIS (GEÇİCİ BLOKAJ) GÜVENLİĞİ: Bu slot şu an başkasının sepetinde mi?
            // Not: Redis Lock metodumuz businessId istiyor. DTO'nda olmadığı için şimdilik 0 veriyoruz. 
            // İleride DTO'ya eklersen burayı createDto.BusinessId olarak güncelleyebilirsin.
            int businessId = 0;

            // Redis'e sor: Bu slot kilitli mi? Kilitliyse kime ait?
            int? lockOwnerId = await _redisLockService.GetLockOwnerAsync(businessId, createDto.ReservationDate, createDto.FieldPriceScheduleId);

            // Eğer kilitli bir slot varsa VE bu kilit (UserId) işlemi yapan kişiye AİT DEĞİLSE reddet!
            if (lockOwnerId.HasValue && lockOwnerId.Value != userId)
            {
                return new ErrorResult("Bu saha şu anda başka bir kullanıcı tarafından ödeme aşamasında. Lütfen 5 dakika sonra tekrar deneyin.");
            }

            // 6. KONTROLLER BAŞARILI: Güvenle rezervasyonu oluştur
            var reservation = new Entities.Concrete.Reservation
            {
                FieldPriceScheduleId = createDto.FieldPriceScheduleId,
                ReservationDate = createDto.ReservationDate,
                FinalPrice = createDto.FinalPrice,
                StatusId = 1, // 1 = Aktif/Onaylandı
                UserId = userId
            };

            _reservationDal.Add(reservation);

            // 7. 🚀 TEMİZLİK: Başarıyla DB'ye kaydedildiği için Redis'teki 5 dakikalık geçici kilidi (sepeti) boşaltıyoruz.
            await _redisLockService.UnlockSlotAsync(businessId, createDto.ReservationDate, createDto.FieldPriceScheduleId);

            await _notificationService.SendSlotBookedNotificationAsync(businessId, createDto.ReservationDate, createDto.FieldPriceScheduleId);


            return new SuccessResult("Rezervasyon başarıyla oluşturuldu.");
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
    }
}

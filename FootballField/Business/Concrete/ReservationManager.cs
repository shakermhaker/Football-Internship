using Business.Abstract;
using Core.Utilities.Results;
using DataAccess.Abstract;
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

        private static readonly object _reservationLock = new object();


        public ReservationManager(IReservationDal reservationDal)
        {
            _reservationDal = reservationDal;
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

        public IResult CreateReservation(CreateReservationDto createDto, int userId)
        {
            // LOCK Mekanizması: Aynı anda birden fazla kişi istek atarsa, biri bitene kadar bekler.
            lock (_reservationLock)
            {

                int requestedDayOfWeek = (int)createDto.ReservationDate.DayOfWeek;
                int requestedDbDayId = requestedDayOfWeek == 0 ? 7 : requestedDayOfWeek;

                int actualSlotDayId = _reservationDal.GetDayIdByScheduleId(createDto.FieldPriceScheduleId);

                // Gönderilen tarih ile, seçilen saatin veritabanındaki günü eşleşmiyor! Hacker var!
                if (actualSlotDayId != requestedDbDayId)
                {
                    return new ErrorResult("Sistem İhlali Tespit Edildi: Seçilen tarih ile rezerve edilmek istenen saatin günü uyuşmuyor!");
                }

                // 1. KONTROL: İçeri giren kişinin istediği slot az önce doldurulmuş mu?
                bool isBooked = _reservationDal.IsSlotBooked(createDto.FieldPriceScheduleId, createDto.ReservationDate);

                if (isBooked)
                {
                    return new ErrorResult("Üzgünüz, bu saha ve saat az önce başka biri tarafından rezerve edildi. Lütfen başka bir saat seçiniz.");
                }

                // 2. KONTROL BAŞARILIYSA: Güvenle rezervasyonu oluştur
                var reservation = new Entities.Concrete.Reservation
                {
                    FieldPriceScheduleId = createDto.FieldPriceScheduleId,
                    ReservationDate = createDto.ReservationDate,
                    FinalPrice = createDto.FinalPrice,
                    StatusId = 1, // 1 = Aktif/Onaylandı
                    UserId = userId  // 🚀 ARTIK TOKEN'DAN GELEN GERÇEK ID KULLANILIYOR!
                };

                _reservationDal.Add(reservation);
                return new SuccessResult("Rezervasyon başarıyla oluşturuldu.");
            }
        }

        public IDataResult<List<UserReservationDetailDto>> GetUserReservations(int userId)
        {
            var data = _reservationDal.GetUserReservations(userId);
            return new SuccessDataResult<List<UserReservationDetailDto>>(data, "Rezervasyon geçmişiniz başarıyla getirildi.");
        }

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
    }
}

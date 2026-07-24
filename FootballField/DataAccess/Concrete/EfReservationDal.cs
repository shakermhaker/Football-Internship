using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using FootballField.DataAccess.Concrete.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Concrete
{
    public class EfReservationDal : EfEntityRepositoryBase<Reservation, FootballFieldContext>, IReservationDal
    {
        // 🚀 Sorgumuzu ReservationDal içine taşıdık ve TimeOnly yapına uydurduk!
        public List<FootballFieldScheduleDto> GetFieldSchedulesByBusinessId(int businessId, int dayId)
        {
            using (var context = new FootballFieldContext())
            {
                var result = context.FootballFields
                    .Where(f => f.BusinessId == businessId)
                    .Select(f => new FootballFieldScheduleDto
                    {
                        FootballFieldId = f.Id,
                        FootballFieldName = f.FieldName,
                        Schedules = f.PriceSchedules
                            .Where(s => s.DayId == dayId) // 🚀 FİLTRE: SADECE SEÇİLEN GÜNÜN SLOTLARI GELİR
                            .OrderBy(s => s.TimeSlot.StartTime)
                            .Select(s => new PriceScheduleDto
                            {
                                FieldPriceScheduleId = s.Id,
                                DayId = s.DayId,
                                DayName = s.Day.Name,
                                TimeSlotId = s.TimeSlotId,
                                StartTime = TimeOnly.FromTimeSpan(s.TimeSlot.StartTime),
                                EndTime = TimeOnly.FromTimeSpan(s.TimeSlot.EndTime),
                                Price = s.Price
                            }).ToList()
                    })
                    .Where(f => f.Schedules.Any()) // 🚀 Eğer o sahada o gün için hiç slot yoksa, o sahayı ekrana boşuna getirme
                    .ToList();

                return result;
            }
        }




        public List<int> GetBookedScheduleIdsByDate(int businessId, DateOnly date)
        {
            using (var context = new FootballFieldContext())
            {
                // Reservations tablosundan, o işletmedeki sahalara ait ve verilen tarihteki rezervasyonları filtreliyoruz
                var bookedIds = context.Reservations
                    .Where(r => r.ReservationDate == date && r.FieldPriceSchedule.FootballField.BusinessId == businessId && r.Status.Id == 1)
                    .Select(r => r.FieldPriceScheduleId)
                    .ToList();

                return bookedIds;
            }
        }

        public bool IsSlotBooked(int fieldPriceScheduleId, DateOnly date)
        {
            using (var context = new FootballFieldContext())
            {
                // Eğer StatusId == 1 olan bir kayıt varsa true döner (Yani Dolu!)
                return context.Reservations.Any(r =>
                    r.FieldPriceScheduleId == fieldPriceScheduleId &&
                    r.ReservationDate == date &&
                    r.StatusId == 1);
            }
        }

        public int GetDayIdByScheduleId(int scheduleId)
        {
            using (var context = new FootballFieldContext())
            {
                var schedule = context.Set<FieldPriceSchedule>().FirstOrDefault(s => s.Id == scheduleId);
                return schedule != null ? schedule.DayId : 0;
            }
        }

        public List<UserReservationDetailDto> GetUserReservations(int userId)
        {
            using (var context = new FootballFieldContext())
            {
                var result = context.Reservations
                    // 1. İlişkili Tabloları Include ile Dahil Ediyoruz
                    .Include(r => r.Status)
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.TimeSlot)
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.FootballField)
                            .ThenInclude(ff => ff.Business)
                                .ThenInclude(b => b.District)
                                    .ThenInclude(c => c.City)
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.FootballField)
                            .ThenInclude(ff => ff.Business)
                                .ThenInclude(b => b.District)

                    // 2. Kullanıcıya Göre Filtreliyoruz
                    .Where(r => r.UserId == userId)

                    // 3. En yeniler en üstte gelsin
                    .OrderByDescending(r => r.ReservationDate)

                    .Select(r => new
                    {
                        ReservationId = r.Id,
                        ReservationDate = r.ReservationDate,
                        StartTime = r.FieldPriceSchedule.TimeSlot.StartTime, // TimeSpan olarak gelir
                        EndTime = r.FieldPriceSchedule.TimeSlot.EndTime,     // TimeSpan olarak gelir
                        FootballFieldName = r.FieldPriceSchedule.FootballField.FieldName,
                        BusinessId = r.FieldPriceSchedule.FootballField.BusinessId,
                        BusinessName = r.FieldPriceSchedule.FootballField.Business.Name,
                        CityName = r.FieldPriceSchedule.FootballField.Business.District.City.Name,
                        DistrictName = r.FieldPriceSchedule.FootballField.Business.District.Name,
                        FinalPrice = r.FinalPrice,
                        StatusName = r.Status.Name
                    })
                    .ToList() // 🚀 SQL SORGUSU BURADA ÇALIŞIR VE BİTER, VERİ RAM'E GELİR

                    // 5. C# Tarafında DTO'ya Çeviriyoruz (TimeSpan -> TimeOnly Dönüşümü Burada Yapılır)
                    .Select(x => new UserReservationDetailDto
                    {
                        ReservationId = x.ReservationId,
                        ReservationDate = x.ReservationDate,
                        StartTime = TimeOnly.FromTimeSpan(x.StartTime), // 🚀 Dönüşüm yapıldı!
                        EndTime = TimeOnly.FromTimeSpan(x.EndTime),     // 🚀 Dönüşüm yapıldı!
                        FootballFieldName = x.FootballFieldName,
                        BusinessId = x.BusinessId,
                        BusinessName = x.BusinessName,
                        CityName = x.CityName,
                        DistrictName = x.DistrictName,
                        FinalPrice = x.FinalPrice,
                        StatusName = x.StatusName
                    }).ToList();

                return result;
            }
        }
    }
}

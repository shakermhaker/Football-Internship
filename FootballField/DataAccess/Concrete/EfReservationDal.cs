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

        public TimeOnly GetStartTimeByScheduleId(int scheduleId)
        {
            using (var context = new FootballFieldContext())
            {
                var schedule = context.FieldPriceSchedules
                                      .Include(f => f.TimeSlot)
                                      .FirstOrDefault(f => f.Id == scheduleId);

                return schedule != null ? TimeOnly.FromTimeSpan(schedule.TimeSlot.StartTime) : TimeOnly.MinValue;
            }
        }

        public List<UserReservationDetailDto> GetUserReservations(int userId)
        {
            using (var context = new FootballFieldContext())
            {
                var result = context.Reservations
                    .Include(r => r.Status)
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.TimeSlot)
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.FootballField)
                            .ThenInclude(ff => ff.Business)
                                .ThenInclude(b => b.District)
                                    .ThenInclude(d => d.City)
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.FootballField)
                            .ThenInclude(ff => ff.Business)
                                .ThenInclude(b => b.District)
                    .Where(r => r.UserId == userId)

                    // 🚀 SIRALAMA BURADA: Önce Tarihe göre, tarih aynıysa Saate göre
                    .OrderByDescending(r => r.ReservationDate)
                    .ThenByDescending(r => r.FieldPriceSchedule.TimeSlot.StartTime)

                    // 4. Verileri Önce Ara Nesneye Çekiyoruz (Süslü parantez eklendi!)
                    .Select(r => new
                    {
                        ReservationId = r.Id,
                        ReservationDate = r.ReservationDate,
                        StartTime = r.FieldPriceSchedule.TimeSlot.StartTime,
                        EndTime = r.FieldPriceSchedule.TimeSlot.EndTime,
                        FootballFieldName = r.FieldPriceSchedule.FootballField.FieldName,
                        BusinessId = r.FieldPriceSchedule.FootballField.BusinessId,
                        BusinessName = r.FieldPriceSchedule.FootballField.Business.Name,
                        CityName = r.FieldPriceSchedule.FootballField.Business.District.City.Name,
                        DistrictName = r.FieldPriceSchedule.FootballField.Business.District.Name,
                        FinalPrice = r.FinalPrice,
                        StatusName = r.Status.Name
                    })
                    .ToList()

                    // 5. TimeSpan'i TimeOnly'ye çevirip DTO'ya aktarıyoruz
                    .Select(x => new UserReservationDetailDto
                    {
                        ReservationId = x.ReservationId,
                        ReservationDate = x.ReservationDate,
                        StartTime = TimeOnly.FromTimeSpan(x.StartTime),
                        EndTime = TimeOnly.FromTimeSpan(x.EndTime),
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

        public BusinessDashboardDto GetBusinessDashboardStats(int businessId, int year)
        {
            using (var context = new FootballFieldContext())
            {
                
                var today = DateOnly.FromDateTime(DateTime.Now);

                
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var startOfWeek = today.AddDays(-diff);

                
                var rawData = context.Reservations
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.FootballField)
                    .Where(r => r.FieldPriceSchedule.FootballField.BusinessId == businessId
                             && r.ReservationDate.Year == year
                             && (r.StatusId == 1 || r.StatusId == 3))
                    .Select(r => new
                    {
                        Date = r.ReservationDate,
                        Price = r.FinalPrice,
                        FieldId = r.FieldPriceSchedule.FootballFieldId,
                        FieldName = r.FieldPriceSchedule.FootballField.FieldName
                    })
                    .ToList(); 
                var dashboardDto = new BusinessDashboardDto();

                dashboardDto.TotalRevenueThisYear = rawData.Sum(r => r.Price);

                dashboardDto.TotalRevenueThisMonth = rawData
                    .Where(r => r.Date.Month == today.Month)
                    .Sum(r => r.Price);

                dashboardDto.TotalRevenueThisWeek = rawData
                    .Where(r => r.Date >= startOfWeek && r.Date <= today)
                    .Sum(r => r.Price);

                dashboardDto.TotalReservationsThisMonth = rawData
                    .Count(r => r.Date.Month == today.Month);

                dashboardDto.FieldRevenues = rawData
                    .GroupBy(r => new { r.FieldId, r.FieldName })
                    .Select(g => new FieldRevenueDto
                    {
                        FieldId = g.Key.FieldId,
                        FieldName = g.Key.FieldName,
                        TotalRevenue = g.Sum(x => x.Price),
                        ReservationCount = g.Count()
                    })
                    .OrderByDescending(f => f.TotalRevenue)
                    .ToList();

                // 4. ADIM: Aylık Gelir Hesaplama (Çubuk Grafik İçin)
                var monthNames = new[] { "", "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

                dashboardDto.FieldRevenues = rawData
                    .GroupBy(r => new { r.FieldId, r.FieldName })
                    .Select(g => new FieldRevenueDto
                    {
                        FieldId = g.Key.FieldId,
                        FieldName = g.Key.FieldName,
                        TotalRevenue = g.Sum(x => x.Price), // O sahanın yıllık toplamı
                        ReservationCount = g.Count(),       // O sahanın yıllık rezervasyon sayısı

                        // 🚀 YENİ: O sahanın kendi içindeki aylık gruplaması
                        MonthlyRevenues = g.GroupBy(x => x.Date.Month)
                            .Select(mg => new MonthlyRevenueDto
                            {
                                Month = mg.Key,
                                MonthName = monthNames[mg.Key],
                                Revenue = mg.Sum(m => m.Price),
                                ReservationCount = mg.Count()
                            })
                            .OrderBy(m => m.Month)
                            .ToList()
                    })
                    .OrderByDescending(f => f.TotalRevenue)
                    .ToList();

                dashboardDto.MonthlyRevenues = rawData
                    .GroupBy(r => r.Date.Month)
                    .Select(g => new MonthlyRevenueDto
                    {
                        Month = g.Key,
                        MonthName = monthNames[g.Key],
                        Revenue = g.Sum(x => x.Price),
                        ReservationCount = g.Count()
                    })
                    .OrderBy(m => m.Month)
                    .ToList();

                // Eğer hiç rezervasyon olmayan aylar varsa onları da sıfır olarak eklemek istersen burada küçük bir for döngüsü yapılabilir (Şimdilik olanları listeler).

                return dashboardDto;
            }
        }

        public DailyReservationSummaryDto GetDailyReservationSummary(int businessId, DateOnly targetDate)
        {
            using (var context = new FootballFieldContext())
            {
                // 1. ADIM: Include ve Lambda ifadeleriyle veriyi çekme ve formatlama
                var rawData = context.Reservations
                    .Include(r => r.User)
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.FootballField)
                    .Include(r => r.FieldPriceSchedule)
                        .ThenInclude(fps => fps.TimeSlot)
                    .Where(r => r.FieldPriceSchedule.FootballField.BusinessId == businessId
                             && r.ReservationDate == targetDate)
                    .Select(r => new
                    {
                        r.Id,
                        r.UserId,
                        FieldName = r.FieldPriceSchedule.FootballField.FieldName,
                        StartTime = r.FieldPriceSchedule.TimeSlot.StartTime,
                        EndTime = r.FieldPriceSchedule.TimeSlot.EndTime,
                        CustomerName = $"{r.User.FirstName} {r.User.LastName}",
                        CustomerPhone = r.User.Phone ?? "",
                        r.FinalPrice,
                        r.StatusId
                    })
                    .ToList();

                // 2. ADIM: Eğer o gün hiç rezervasyon yoksa boş DTO dön
                if (!rawData.Any())
                {
                    return new DailyReservationSummaryDto();
                }

                // 3. ADIM: Çekilen ham veriyi senin Angular tarafında beklediğin DTO formatına çevirme
                var reservationList = rawData.Select(x => new DailyReservationDetailDto
                {
                    Id = x.Id,
                    FieldName = x.FieldName,
                    TimeInterval = $"{x.StartTime:hh\\:mm} - {x.EndTime:hh\\:mm}",
                    CustomerName = x.CustomerName,
                    CustomerPhone = x.CustomerPhone,
                    FinalPrice = x.FinalPrice,
                    StatusId = x.StatusId
                }).ToList();

                // 4. ADIM: İstatistik kartları için hesaplamalar (Min, Max, Unique Müşteri vs.)
                int totalReservations = rawData.Count;
                int uniqueCustomers = rawData.Select(x => x.UserId).Distinct().Count();

                var minTime = rawData.Min(x => x.StartTime);
                var maxTime = rawData.Max(x => x.EndTime);
                string earliestAndLatest = $"{minTime:hh\\:mm} - {maxTime:hh\\:mm}";

                // 5. ADIM: Paketi topla ve gönder!
                return new DailyReservationSummaryDto
                {
                    TotalReservations = totalReservations,
                    EarliestAndLatestTime = earliestAndLatest,
                    TotalUniqueCustomers = uniqueCustomers,
                    Reservations = reservationList
                };
            }
        }
    }
}

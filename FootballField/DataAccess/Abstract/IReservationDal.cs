using Core.DataAccess;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Abstract
{
    public interface IReservationDal : IEntityRepository<Reservation>
    {
        // 🚀 İşletmenin sahalarına ait takvimi çeken özel metot
       
        bool IsSlotBooked(int fieldPriceScheduleId, DateOnly date);
        int GetDayIdByScheduleId(int scheduleId);
        List<UserReservationDetailDto> GetUserReservations(int userId);

        BusinessDashboardDto GetBusinessDashboardStats(int businessId, int year);
        TimeOnly GetStartTimeByScheduleId(int scheduleId);
        DailyReservationSummaryDto GetDailyReservationSummary(int businessId, DateOnly targetDate);
        void CompensateUsersForScheduleChange(List<int> deletedFieldPriceScheduleIds);
        int GetFreeRightCount(int userId, int footballFieldId);
        bool TryUseFreeRight(int userId, int fieldPriceScheduleId);
        List<SlotStateDto> GetBookedSlotsByDateRange(int businessId, DateOnly startDate, DateOnly endDate);
        List<FootballFieldScheduleDto> GetAllWeeklySchedulesByBusinessId(int businessId);
    }
}

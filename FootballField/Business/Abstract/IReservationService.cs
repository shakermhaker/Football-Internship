using Core.Utilities.Results;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IReservationService
    {
        // İşletmenin sahalarını ve fiyat/saat takvimini getirir
        Task<IResult> CancelHoldSlotAsync(int businessId, DateOnly date, int scheduleId, int userId);

        Task<IResult> CreateReservationAsync(CreateReservationDto createDto, int userId);
        Task<IResult> HoldReservationSlotAsync(int businessId, DateOnly date, int scheduleId, int userId);
        IDataResult<List<UserReservationDetailDto>> GetUserReservations(int userId);

        IResult CancelReservation(int reservationId, int userId);

        IDataResult<DailyReservationSummaryDto> GetDailyReservations(int businessId, DateTime date);
        IResult CancelReservationByBusiness(int reservationId);
        IDataResult<int> CheckFreeBookingRights(int userId, int footballFieldId);
        IResult UseFreeBookingRight(int reservationId);

        IDataResult<List<FootballFieldScheduleDto>> GetAllWeeklySchedules(int businessId);
        IDataResult<List<SlotStateDto>> GetBookedSlotsByDateRange(int businessId, DateOnly startDate, DateOnly endDate);
        Task<IDataResult<List<SlotStateDto>>> GetHeldSlotsByDateRangeAsync(int businessId, DateOnly startDate, DateOnly endDate);

    }
}

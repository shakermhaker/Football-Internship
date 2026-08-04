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
        IDataResult<List<FootballFieldScheduleDto>> GetBusinessFieldSchedules(int businessId, DateOnly date);
        IDataResult<List<int>> GetBookedScheduleIdsByDate(int businessId, DateOnly date);

        Task<IResult> CreateReservationAsync(CreateReservationDto createDto, int userId);
        Task<IResult> HoldReservationSlotAsync(int businessId, DateOnly date, int scheduleId, int userId);
        IDataResult<List<UserReservationDetailDto>> GetUserReservations(int userId);

        IResult CancelReservation(int reservationId, int userId);

        IDataResult<DailyReservationSummaryDto> GetDailyReservations(int businessId, DateTime date);
        IResult CancelReservationByBusiness(int reservationId);

    }
}

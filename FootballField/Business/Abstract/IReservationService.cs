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

        IResult CreateReservation(CreateReservationDto createDto, int userId);



    }
}

using Core.DataAccess;
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
        List<FootballFieldScheduleDto> GetFieldSchedulesByBusinessId(int businessId, int dayId);
        List<int> GetBookedScheduleIdsByDate(int businessId, DateOnly date);
        bool IsSlotBooked(int fieldPriceScheduleId, DateOnly date);
        int GetDayIdByScheduleId(int scheduleId);



    }
}

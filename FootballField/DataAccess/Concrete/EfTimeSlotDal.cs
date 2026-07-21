using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete;
using FootballField.DataAccess.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Concrete
{
    public class EfTimeSlotDal : EfEntityRepositoryBase<TimeSlot, FootballFieldContext>, ITimeSlotDal
    {
        public TimeSlot GetByTimes(TimeSpan startTime, TimeSpan endTime)
        {
            using (var context = new FootballFieldContext())
            {
                return context.TimeSlots.FirstOrDefault(t => t.StartTime == startTime && t.EndTime == endTime);
            }
        }
    }
}

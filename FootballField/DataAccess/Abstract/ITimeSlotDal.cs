using Core.DataAccess;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;
using Core.DataAccess;
using Entities.Concrete;
using System;

namespace DataAccess.Abstract
{
    public interface ITimeSlotDal : IEntityRepository<TimeSlot>
    {
        // BAK BURASI ÇOK ÖNEMLİ: string değil, TimeSpan olacak!
        TimeSlot GetByTimes(TimeSpan startTime, TimeSpan endTime);
    }
}
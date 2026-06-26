using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class TimeSlot : IEntity
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Navigation Property
        public ICollection<FieldPriceSchedule> FieldPriceSchedules { get; set; } = new List<FieldPriceSchedule>();
    }
}

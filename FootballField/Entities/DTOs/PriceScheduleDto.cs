using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class PriceScheduleDto
    {
        public int FieldPriceScheduleId { get; set; }

        // Gün Bilgileri
        public int DayId { get; set; }
        public string DayName { get; set; }

        public int TimeSlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Fiyat
        public decimal Price { get; set; }
    }
}

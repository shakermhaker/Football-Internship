using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class MonthlyRevenueDto
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public decimal Revenue { get; set; }
        public int ReservationCount { get; set; }
    }
}

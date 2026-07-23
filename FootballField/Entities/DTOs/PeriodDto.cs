using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class PeriodDto : IDto 
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
    }
}

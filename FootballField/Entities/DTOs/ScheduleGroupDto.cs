using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class ScheduleGroupDto : IDto
    {
        public List<int> SelectedDayIds { get; set; }
        public List<PeriodDto> Periods { get; set; }
    }
}

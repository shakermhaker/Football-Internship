using System;
using System.Collections.Generic;
using System.Text;
using Core.Entities; 


namespace Entities.DTOs
{
    public class FootballFieldAddDTO : IDto
    {
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public List<ScheduleGroupDto> ScheduleGroups { get; set; }
    }

    public class ScheduleGroupDto
    {
        public List<int> SelectedDayIds { get; set; }
        public List<PeriodDto> Periods { get; set; }
    }
    public class PeriodDto
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
    }
}

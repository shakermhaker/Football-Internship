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
    
}

using System;
using System.Collections.Generic;
using System.Text;


namespace Entities.DTOs
{
    public class FootballFieldScheduleDto
    {
        public int FootballFieldId { get; set; }
        public string FootballFieldName { get; set; }

        // O sahaya ait tüm gün/saat/fiyat listesi
        public List<PriceScheduleDto> Schedules { get; set; } = new List<PriceScheduleDto>();
    }
}

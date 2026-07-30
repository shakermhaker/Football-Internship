using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class DailyReservationDetailDto : IDto
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string TimeInterval { get; set; } = string.Empty; // Örn: "16:00 - 17:00"
        public string CustomerName { get; set; } = string.Empty;  // Örn: "Mehmet Yılmaz"
        public string CustomerPhone { get; set; } = string.Empty; // Örn: "0555 123 45 67"
        public decimal FinalPrice { get; set; }
        public int StatusId { get; set; }
    }
}

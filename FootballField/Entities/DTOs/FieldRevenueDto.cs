using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class FieldRevenueDto
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ReservationCount { get; set; }

        // 🚀 YENİ: O sahanın kendi içindeki aylık kazanç dağılımı!
        public List<MonthlyRevenueDto> MonthlyRevenues { get; set; } = new List<MonthlyRevenueDto>();
    }
}

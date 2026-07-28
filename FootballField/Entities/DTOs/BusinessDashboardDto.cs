using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class BusinessDashboardDto
    {
        public decimal TotalRevenueThisYear { get; set; }
        public decimal TotalRevenueThisMonth { get; set; }
        public decimal TotalRevenueThisWeek { get; set; }

        public int TotalReservationsThisMonth { get; set; }

        // Halı Saha bazlı gelir dağılımı (Örn: Pasta Grafik için)
        public List<FieldRevenueDto> FieldRevenues { get; set; } = new List<FieldRevenueDto>();

        // Aylık gelir dağılımı (Örn: Çubuk Grafik için - Ocak, Şubat, Mart...)
        public List<MonthlyRevenueDto> MonthlyRevenues { get; set; } = new List<MonthlyRevenueDto>();
    }
}

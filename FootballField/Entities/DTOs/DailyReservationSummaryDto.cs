using System;
using System.Collections.Generic;
using System.Text;
using Core.Entities;

namespace Entities.DTOs 
{
    public class DailyReservationSummaryDto : IDto
    {
        public int TotalReservations { get; set; }           // Kart 1: Toplam Rezervasyon
        public string EarliestAndLatestTime { get; set; } = "-"; // Kart 2: "16:00 - 24:00"
        public int TotalUniqueCustomers { get; set; }       // Kart 3: Toplam Müşteri
        public List<DailyReservationDetailDto> Reservations { get; set; } = new();

    }
}

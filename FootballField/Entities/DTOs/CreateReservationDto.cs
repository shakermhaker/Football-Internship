using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class CreateReservationDto
    {
        public int FieldPriceScheduleId { get; set; }
        public DateOnly ReservationDate { get; set; }
        public decimal FinalPrice { get; set; }
        public string CardNumber { get; set; } // Şimdilik ödeme almadığımız için temsili
    }
}

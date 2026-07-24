using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class UserReservationDetailDto
    {
        public int ReservationId { get; set; }
        public DateOnly ReservationDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string FootballFieldName { get; set; }
        public int BusinessId { get; set; } // İşletmeye tıklandığında yönlendirmek için
        public string BusinessName { get; set; }
        public string CityName { get; set; }
        public string DistrictName { get; set; }
        public decimal FinalPrice { get; set; }
        public string StatusName { get; set; }
    }
}

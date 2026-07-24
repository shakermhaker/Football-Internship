using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization; // Bunu yukarı eklemeyi unutmayın
namespace Entities.Concrete
{
    public class FieldPriceSchedule : IEntity
    {
        public int Id { get; set; }
        public decimal Price { get; set; }

        // Foreign Keys
        public int FootballFieldId { get; set; }
        [JsonIgnore]
        public FootballField FootballField { get; set; } = null!;


        public int TimeSlotId { get; set; }
        public TimeSlot TimeSlot { get; set; } = null!;

        public int DayId { get; set; } // Yeni eklenen Days tablosu ile ilişki
        public Day Day { get; set; } = null!;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class Status : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Diyagramdaki Context (Örn: "Onaylandı", "Bekliyor", "İptal")

        // Navigation Property
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}

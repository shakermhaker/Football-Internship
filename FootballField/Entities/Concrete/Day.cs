using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class Day : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Diyagramdaki Context (Pazartesi, Salı vb.)

        // Navigation Property
        public ICollection<FieldPriceSchedule> FieldPriceSchedules { get; set; } = new List<FieldPriceSchedule>();
    }
}

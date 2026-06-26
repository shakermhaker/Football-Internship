using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class City : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Diyagramdaki Context

        // Navigation Property
        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}

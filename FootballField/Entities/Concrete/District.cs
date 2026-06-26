using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class District : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Diyagramdaki Context

        // Foreign Key
        public int CityId { get; set; }
        public City City { get; set; } = null!;

        // Navigation Property
        public ICollection<Business> Businesses { get; set; } = new List<Business>();
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class Business : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public int DistrictId { get; set; }
        public District District { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public bool IsApproved { get; set; } = false;

        // Navigation Properties
        public ICollection<FootballField> FootballFields { get; set; } = new List<FootballField>();
        public ICollection<BusinessImage> BusinessImages { get; set; } = new List<BusinessImage>();
    }
}

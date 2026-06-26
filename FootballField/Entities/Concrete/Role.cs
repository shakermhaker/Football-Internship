using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class Role : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Diyagramdaki Context

        // Navigation Property
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

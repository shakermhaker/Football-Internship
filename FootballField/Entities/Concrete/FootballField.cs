using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Entities.Concrete
{
    public class FootballField : IEntity
    {
        public int Id { get; set; }
        public string FieldName { get; set; } = string.Empty;

        // Foreign Key
        public int BusinessId { get; set; }
        public Business Business { get; set; } = null!;

        // Navigation Properties
        public ICollection<FieldPriceSchedule> PriceSchedules { get; set; } = new List<FieldPriceSchedule>();
        
        
    }
}

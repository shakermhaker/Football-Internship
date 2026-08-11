using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public abstract class BaseEntity : IEntity, IAuditableEntity
    {
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }  
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}

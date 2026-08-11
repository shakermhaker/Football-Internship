using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Entities
{
    public interface IAuditableEntity
    {
        DateTime? CreatedAt { get; set; }
        int? CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        int? UpdatedBy { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Core.Entities;
namespace Entities.Concrete
{
    public class TestEntity : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

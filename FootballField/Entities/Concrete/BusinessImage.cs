using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Concrete
{
    public class BusinessImage : BaseEntity
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        // Foreign Key
        public int BusinessId { get; set; }
        public Business Business { get; set; } = null!;
        public bool IsCover { get; set; } = false;
    }   
}


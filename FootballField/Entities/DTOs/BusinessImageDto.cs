using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class BusinessImageDto : IDto
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public bool IsCover { get; set; }
    }
}

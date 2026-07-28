using Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.DTOs
{
    public class BusinessUpdateDto : IDto
    {
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public int DistrictId { get; set; } // Veritabanında ilçe ID'si tutulduğu için bu gerekli
        public string FullAddress { get; set; }
    }
}

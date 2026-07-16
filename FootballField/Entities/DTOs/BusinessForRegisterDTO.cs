using System;
using System.Collections.Generic;
using System.Text;
using Core.Entities;

namespace Entities.DTOs
{
    public class BusinessForRegisterDTO : IDto
    {
        public string Name { get; set; }
        public string FullAddress { get; set; }
        public int DistrictId { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Core.Entities; // Kendi core katmanındaki IEntity/IDto yoluna göre ayarla

namespace Entities.DTOs
{
    public class CityDTO : IDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Entities.Concrete 
{
    public class Reservation : IEntity
    {
        public int Id { get; set; }
        public decimal FinalPrice { get; set; }

        // Foreign Keys

        // Yeni eklenen Status tablosu ile olan bağlantı
        public int StatusId { get; set; }
        public Status Status { get; set; } = null!;

        // Önceki şemalardan gelen mevcut bağlantılar
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int FieldPriceScheduleId { get; set; }
        public FieldPriceSchedule FieldPriceSchedule { get; set; } = null!;
    }
}

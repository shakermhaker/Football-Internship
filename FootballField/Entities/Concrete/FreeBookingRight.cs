using System;
using System.Collections.Generic;
using System.Text;
using Core.Entities;

namespace Entities.Concrete
{
    public class FreeBookingRight : IEntity
    {
        public int Id { get; set; }

        // Hangi kullanıcının hakkı var?
        public int UserId { get; set; }

        // Bu hak HANGİ işletmede geçerli? (O işletmenin tüm sahalarında kullanılabilir)
        public int BusinessId { get; set; }

        // Bu bedava hak, tamamlanmış (status = 4) hangi rezervasyondan kazanıldı?
        public int ReservationId { get; set; }

        // Bu hak kullanıldı mı? (Her kazanılan hak ayrı satır olduğu için true/false ile takip edilecek)
        public bool IsUsed { get; set; }

        // Navigation Properties (Tabloları birbirine bağlamak için)
        public User User { get; set; } = null!;
        public Business Business { get; set; } = null!; // İşletme tablonun adının Business olduğunu varsaydım
        public Reservation Reservation { get; set; } = null!;
    }
}

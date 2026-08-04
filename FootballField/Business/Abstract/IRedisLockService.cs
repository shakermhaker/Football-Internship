using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IRedisLockService
    {
        // 🚀 userId parametresi int'e çevrildi
        Task<bool> LockSlotAsync(int businessId, DateOnly date, int scheduleId, int userId, int expiryMinutes = 5);

        // Kullanıcı ödemeyi tamamlarsa veya iptal ederse kilidi manuel olarak kaldırır
        Task UnlockSlotAsync(int businessId, DateOnly date, int scheduleId);

        // 🚀 Geri dönüş tipi int? (Nullable int) yapıldı. Kilit yoksa null, varsa UserId dönecek.
        Task<int?> GetLockOwnerAsync(int businessId, DateOnly date, int scheduleId);
    }
}

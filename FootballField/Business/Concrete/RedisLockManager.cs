using Business.Abstract;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Concrete
{
    public class RedisLockManager : IRedisLockService
    {
        private readonly IDistributedCache _cache;

        public RedisLockManager(IDistributedCache cache)
        {
            _cache = cache;
        }

        private string GenerateKey(int businessId, DateOnly date, int scheduleId)
        {
            // Örnek Çıktı: "Hold:2:2026-08-04:456"
            return $"Hold:{businessId}:{date.ToString("yyyy-MM-dd")}:{scheduleId}";
        }

        public async Task<bool> LockSlotAsync(int businessId, DateOnly date, int scheduleId, int userId, int expiryMinutes = 5)
        {
            var key = GenerateKey(businessId, date, scheduleId);

            // Önce bu slot zaten başkası tarafından kilitlenmiş mi diye bakıyoruz
            var existingLock = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(existingLock))
            {
                return false; // Zaten kilitli, işlemi reddet
            }

            // Redis'e kaydedilecek ayarlar (Sadece 5 dakika yaşasın, sonra kendini yok etsin)
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiryMinutes)
            };

            // 🚀 ÇÖZÜM: userId.ToString() yaparak Redis'in beklediği string formatına uygun hale getirdik.
            await _cache.SetStringAsync(key, userId.ToString(), options);
            return true;
        }

        public async Task UnlockSlotAsync(int businessId, DateOnly date, int scheduleId)
        {
            var key = GenerateKey(businessId, date, scheduleId);

            await _cache.RemoveAsync(key);
        }

        public async Task<int?> GetLockOwnerAsync(int businessId, DateOnly date, int scheduleId)
        {
            var key = GenerateKey(businessId, date, scheduleId);

            var ownerIdString = await _cache.GetStringAsync(key);

            // Eğer Redis'te değer varsa onu güvenli bir şekilde int'e çevirip dönüyoruz
            if (int.TryParse(ownerIdString, out int parsedUserId))
            {
                return parsedUserId;
            }

            return null; // Key yoksa veya int'e çevrilemiyorsa null döner
        }

        public async Task<List<int>> GetActiveHoldsAsync(int businessId, DateOnly date, List<int> scheduleIdsToCheck)
        {
            var heldSlots = new List<int>();

            foreach (var scheduleId in scheduleIdsToCheck)
            {
                var key = GenerateKey(businessId, date, scheduleId);
                var existingLock = await _cache.GetStringAsync(key);

                // Eğer Redis'te bu anahtar varsa (null değilse), demek ki işlemde (turuncu)
                if (!string.IsNullOrEmpty(existingLock))
                {
                    heldSlots.Add(scheduleId);
                }
            }

            return heldSlots;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IReservationNotificationService
    {
        // 🚀 DateOnly parametreleri eklendi!
        Task SendSlotHeldNotificationAsync(int businessId, DateOnly date, int scheduleId);
        Task SendSlotBookedNotificationAsync(int businessId, DateOnly date, int scheduleId);
        Task SendSlotUnlockedNotificationAsync(int businessId, DateOnly date, int scheduleId);
    }
}

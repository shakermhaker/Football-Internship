using Business.Abstract;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebAPI.Hubs;

namespace WebAPI.SignalR
{
    public class ReservationNotificationManager : IReservationNotificationService
    {
        private readonly IHubContext<ReservationHub> _hubContext;

        public ReservationNotificationManager(IHubContext<ReservationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendSlotHeldNotificationAsync(int businessId, DateOnly date, int scheduleId)
        {
            // 🚀 BÜYÜ BURADA: Angular tarafında ayrım yapılabilmesi için objenin içine Date string'ini de koyduk.
            await _hubContext.Clients.Group($"Business_{businessId}")
                             .SendAsync("SlotHeld", new
                             {
                                 scheduleId = scheduleId,
                                 date = date.ToString("yyyy-MM-dd")
                             });
        }

        public async Task SendSlotBookedNotificationAsync(int businessId, DateOnly date, int scheduleId)
        {
            await _hubContext.Clients.Group($"Business_{businessId}")
                             .SendAsync("SlotBooked", new
                             {
                                 scheduleId = scheduleId,
                                 date = date.ToString("yyyy-MM-dd")
                             });
        }

        public async Task SendSlotUnlockedNotificationAsync(int businessId, DateOnly date, int scheduleId)
        {
            await _hubContext.Clients.Group($"Business_{businessId}")
                             .SendAsync("SlotUnlocked", new
                             {
                                 scheduleId = scheduleId,
                                 date = date.ToString("yyyy-MM-dd")
                             });
        }
    }
}

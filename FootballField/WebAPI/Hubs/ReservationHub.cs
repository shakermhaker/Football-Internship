using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebAPI.Hubs
{
    public class ReservationHub : Hub
    {
        
        public async Task JoinBusinessGroup(string businessId)
        {
            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Business_{businessId}");
        }

        
        public async Task LeaveBusinessGroup(string businessId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Business_{businessId}");
        }
    }
}

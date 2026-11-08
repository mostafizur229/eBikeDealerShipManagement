using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace eBike.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public async Task JoinConcernGroup(string concernId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Concern_{concernId}");
    }

    public async Task LeaveConcernGroup(string concernId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Concern_{concernId}");
    }

    public async Task JoinServiceTicketRoom(string ticketNumber)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Ticket_{ticketNumber}");
    }

    public async Task LeaveServiceTicketRoom(string ticketNumber)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Ticket_{ticketNumber}");
    }
}

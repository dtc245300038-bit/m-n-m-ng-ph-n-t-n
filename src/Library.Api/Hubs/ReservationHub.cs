
using Microsoft.AspNetCore.SignalR;

namespace Library.Api.Hubs;

public class ReservationHub : Hub
{
    // Cho nguoi dung tham gia nhom theo User ID
    public async Task JoinUserGroup(int userId)
    {
        if (userId <= 0)
        {
            throw new HubException("User ID khong hop le");
        }

        string groupName = $"user-{userId}";

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            groupName
        );

        await Clients.Caller.SendAsync(
            "JoinedGroup",
            $"Da tham gia nhom cua User {userId}"
        );
    }

    // Roi nhom
    public async Task LeaveUserGroup(int userId)
    {
        string groupName = $"user-{userId}";

        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            groupName
        );
    }
}

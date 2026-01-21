using Microsoft.AspNetCore.SignalR;

namespace RealTimeDemo.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private string _lastConnectedId = "";

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public async Task SendMessage(string message, string user)
    {
        if (message.StartsWith("[-COMMAND]"))
        {
            message = message.Replace("-COMMAND", "");

            switch (message)
            {
                case "leavegroup1":
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, _lastConnectedId);
                    return;
                case "joingroup1":
                    await Groups.AddToGroupAsync(Context.ConnectionId, "group1");
                    return;
                case "leavegroup2":
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, _lastConnectedId);
                    return;
                case "groupbroadcastdemo":
                    var groupMessage = "Hello group 1";
                    await Clients.Group("group1").SendAsync("ReceiveMessage", groupMessage);
                    return;
                default:
                    return;
            }
        }
        await Clients.All.SendAsync("ReceiveMessage", message, user);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client Connected {Context.ConnectionId}");
        await SendPrivateMessage(Context.ConnectionId, "SERVER: Welcome to the chat!");
        await base.OnConnectedAsync();
    }

    public async Task RegisterClient(string groupName, string connectionName)
    {
        await ConnectionState.AddConnectionName(Context.ConnectionId, connectionName);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await ConnectionState.AddConnectionToGroup(Context.ConnectionId, groupName);
        var onlineConnectionNamesInGroup = await ConnectionState.GetOnlineUsersForGroup(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UpdateConnectionNames", onlineConnectionNamesInGroup);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendToClientConnection(string connectionId, string message)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
    }

    public async Task SendToUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveMessage", message);
    }

    public async Task SendToGroup(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
    }

    public async Task BroadcastToAll(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    public async Task BroadcastToAllExceptSender(string message)
    {
        await Clients.Others.SendAsync("ReceiveMessage", message);
    }

    public async Task SendPrivateMessage(string connectionId, string message)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var errorMessage = exception?.Message ?? "No error";

        var groupsToUpdate = await ConnectionState.GetGroupsForConnectionId(Context.ConnectionId);

        await ConnectionState.RemoveConnectionFromAllGroups(Context.ConnectionId);
        await ConnectionState.RemoveConnectionName(Context.ConnectionId);
        groupsToUpdate.ForEach(async x =>
        {
            var onlineGroupUsers = await ConnectionState.GetOnlineUsersForGroup(Context.ConnectionId, x);
            await Clients.Group(x).SendAsync("UpdateConnectionNames", onlineGroupUsers);
        });
        _logger.LogInformation($"Client disconnected {Context.ConnectionId}. Error {errorMessage}");
        await base.OnDisconnectedAsync(exception);
    }
}
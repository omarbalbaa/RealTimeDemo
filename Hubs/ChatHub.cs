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
        var groupName = "Group 1";
        await JoinGroup(groupName);
        await SendPrivateMessage(Context.ConnectionId, "SERVER: Welcome to the chat!");
        await SendPrivateMessage(Context.ConnectionId, $"SERVER: You have been added to {groupName}");
        await base.OnConnectedAsync();
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
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
        _logger.LogInformation($"Client disconnected {Context.ConnectionId}. Error {errorMessage}");
        await base.OnDisconnectedAsync(exception);
    }
}
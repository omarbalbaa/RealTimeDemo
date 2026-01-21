using Microsoft.AspNetCore.SignalR;

namespace RealTimeDemo.Hubs;

public class StronglyTypedChatHub : Hub<IStronglyTypedChatHub>
{
    private readonly ILogger<StronglyTypedChatHub> _logger;

    public StronglyTypedChatHub(ILogger<StronglyTypedChatHub> logger)
    {
        _logger = logger;
    }

    public async Task SendMessage(string message, string user)
    {
        await Clients.All.ReceiveMessage(message, user);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Client Connected {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var errorMessage = exception?.Message ?? "No error";
        _logger.LogInformation($"Client disconnected {Context.ConnectionId}. Error {errorMessage}");
        await base.OnDisconnectedAsync(exception);
    }
}
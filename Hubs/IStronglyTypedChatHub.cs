namespace RealTimeDemo.Hubs;
public interface IStronglyTypedChatHub
{
    Task ReceiveMessage(string message, string user);
}
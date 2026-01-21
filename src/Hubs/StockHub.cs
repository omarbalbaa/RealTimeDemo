using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR;

namespace RealTimeDemo.Hubs;

public class StockHub : Hub
{
    public async Task UploadNumber(ChannelReader<int> reader)
    {
        await foreach (var number in reader.ReadAllAsync())
        {
            Console.WriteLine($"Received number: {number}");
        }
    }
    public async IAsyncEnumerable<object> StreamStockPrices([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var random = new Random();
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return new StockPrices()
            {
                Symbol = "MSFT",
                Price = Math.Round(250 + random.NextDouble() * 2, 2),
                TimeStamp = DateTime.UtcNow
            };

            await Task.Delay(1000, cancellationToken);
        }
    }
}

public record StockPrices
{
    public string? Symbol {get; set;}
    public double Price {get; set;}
    public DateTime TimeStamp {get; set;}
}
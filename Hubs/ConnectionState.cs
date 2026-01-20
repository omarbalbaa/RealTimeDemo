using System.Collections.Concurrent;

namespace RealTimeDemo.Hubs;

public static class ConnectionState
{
    private static readonly ConcurrentDictionary<string, List<string>> _groups = new();
    public async static Task AddConnectionToGroup(string connectionId, string groupName)
    {
        var groupEntry = _groups.GetOrAdd(groupName, new List<string>());
        lock (groupEntry)
        {
            groupEntry.Add(connectionId);
        }
    }

    public async static Task RemoveConnectionFromGroup(string connectionId, string groupName)
    {
        if (_groups.TryGetValue(groupName, out var group))
        {
            lock (group)
            {
                group.Remove(connectionId);
            }
        }
    }
}
using System.Collections.Concurrent;
using System.Security.Claims;

namespace RealTimeDemo.Hubs;

public static class ConnectionState
{
    private static readonly ConcurrentDictionary<string, List<string>> _groups = new();
    private static readonly ConcurrentDictionary<string, string> _connectionNames = new ();
    public static ConcurrentBag<ClaimsPrincipal> OnlineUsers { get; } = new ();
    public async static Task<List<string>> GetGroupsForConnectionId(string connectionId)
    {
        var groups = _groups.Where(x => x.Value.Contains(connectionId)).Select(z => z.Key).ToList();
        return groups;
    }
    public async static Task<List<string>> GetOnlineUsersForGroup(string connectionId, string groupName)
    {
        var connectionIds = _groups[groupName];
        var onlineUsers = new List<string>();
        foreach (var onlineConnectionId in connectionIds)
        {
            var userName = _connectionNames.ContainsKey(onlineConnectionId) ?
                _connectionNames[onlineConnectionId]
                : "Unknown User";
            onlineUsers.Add(userName);
        }
        return onlineUsers;
    }
    public async static Task AddConnectionName(string connectionId, string name)
    {
        _connectionNames.GetOrAdd(connectionId, name);
    }
    public static async Task RemoveConnectionName(string connectionId)
    {
        _connectionNames.Remove(connectionId, out var value);
    }
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

    public async static Task RemoveConnectionFromAllGroups(string connectionId)
    {
        var groupsForThisConnection = _groups
        .Where(x => x.Value.Contains(connectionId))
        .Select(y => y.Key)
        .ToList();
    }
}
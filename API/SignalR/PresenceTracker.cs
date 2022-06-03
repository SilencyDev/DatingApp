namespace API.SignalR;

public class PresenceTracker
{
    private static readonly Dictionary<string, List<string>> OnlineUsers
	= new Dictionary<string, List<string>>();
	
	public Task<bool> UserConnected(string username, string connectionId) {
		bool firstConnection = false;
		lock (OnlineUsers)
		{
			if (OnlineUsers.ContainsKey(username)) {
				OnlineUsers[username].Add(connectionId);
			} else {
				firstConnection = true;
				OnlineUsers.Add(username, new List<string>{connectionId});
			}
		}
		
		return Task.FromResult(firstConnection);
	}
	
	public Task<bool> UserDisconnected(string username, string connectionId) {
		bool isOffline = false;
		lock(OnlineUsers)
		{
			if (!OnlineUsers.ContainsKey(username))
				return Task.FromResult(isOffline);
				
			OnlineUsers[username].Remove(connectionId);
			
			if (OnlineUsers[username].Count() == 0) {
				isOffline = true;
				OnlineUsers.Remove(username);
			}
		}
		
		return Task.FromResult(isOffline);
	}
	
	public Task<string[]> GetOnlineUsers() {
		string[] onlineUsers;
		lock(OnlineUsers)
		{
			onlineUsers = OnlineUsers
				.OrderBy(k => k.Key)
				.Select(k => k.Key)
				.ToArray();
		}
		
		return Task.FromResult(onlineUsers);
	}
	
	public Task<List<string>> GetConnectionsForUser(string username) {
		List<string> connectionIds;
		lock(OnlineUsers)
		{
			connectionIds = OnlineUsers.GetValueOrDefault(username);
		}
		
		return Task.FromResult(connectionIds);
	}
}
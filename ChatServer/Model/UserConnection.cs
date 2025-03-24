namespace ChatServer.Model;

public class UserConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool IsConnected { get; set; }

    public UserConnection(string connectionId, string username, bool isConnected)
    {
        ConnectionId = connectionId;
        Username = username;
        IsConnected = isConnected;
    }
}
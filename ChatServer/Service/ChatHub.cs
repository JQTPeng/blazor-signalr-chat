using System.Collections.Immutable;
using Microsoft.AspNetCore.SignalR;
using ChatServer.Model;

namespace ChatServer.Service;

public class ChatHub(ILogger<ChatHub> logger) : Hub
{
    private static readonly ImmutableArray<UserConnection> UserConnections =
    [
        new(string.Empty, "Lion", false),
        new(string.Empty, "Tiger", false),
        new(string.Empty, "Elephant", false),
        new(string.Empty, "Giraffe", false),
        new(string.Empty, "Zebra", false),
        new(string.Empty, "Panda", false),
        new(string.Empty, "Kangaroo", false),
        new(string.Empty, "Dolphin", false),
        new(string.Empty, "Eagle", false),
        new(string.Empty, "Wolf", false)
    ];

    public override async Task OnConnectedAsync()
    {
        // Get Empty UserConnection
        var user = UserConnections.FirstOrDefault(connection =>
            !connection.IsConnected &&
            connection.ConnectionId == string.Empty &&
            connection.ConnectionId != Context.ConnectionId);

        // Inform client no available space
        if (user is null)
        {
            await Clients.Caller.SendAsync("OnConnectionRejected", "No available connection slots");
            Context.Abort();
            return;
        }

        // Setup Connection
        user.ConnectionId = Context.ConnectionId;
        user.IsConnected = true;
        await Clients.Caller.SendAsync("OnConnected", user.Username);
        logger.LogInformation("Connection Status: {status}, {context}", "ClientConnected", Context.ConnectionId);

        DisplayUserConnections();
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        // Set User Connection as available
        var user = UserConnections.FirstOrDefault(connection => connection.ConnectionId == Context.ConnectionId);
        if (user is not null)
        {
            user.ConnectionId = string.Empty;
            user.IsConnected = false;
        }

        logger.LogInformation("Connection Status: {status}, {context}", "Disconnected", Context.ConnectionId);

        DisplayUserConnections();
        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        logger.LogInformation("Connection Status: {status}, {context}", "SendMessage", Context.ConnectionId);
        await Clients.All.SendAsync("OnReceiveMessage", user, message);
    }

    public async Task SendHubData()
    {
        await Clients.All.SendAsync("OnSeer");
    }

    /// <summary>
    /// Debugging purposes only
    /// </summary>
    private void DisplayUserConnections()
    {
        foreach (var u in UserConnections)
        {
            Console.WriteLine($"{u.ConnectionId} : {u.Username} : {u.IsConnected}");
        }

        Console.WriteLine();
    }
}
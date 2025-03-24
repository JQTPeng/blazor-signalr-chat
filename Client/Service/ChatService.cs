using Microsoft.AspNetCore.SignalR.Client;
using Client.Enums;

namespace Client.Service;

internal class SignalRRetryPolicy : IRetryPolicy
{
    public TimeSpan? NextRetryDelay(RetryContext retryContext)
    {
        return TimeSpan.FromSeconds(5);
    }
}

public class ChatService(ILogger<ChatService> logger)
{
    private HubConnection? _hubConnection;
    public event Action<string, string> OnMessageReceived;
    public event Action<ChatStatus> OnStatusChanged;
    public string User = string.Empty;

    public async Task ConnectAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .AddJsonProtocol(opts => { opts.PayloadSerializerOptions.PropertyNamingPolicy = null; })
            .WithUrl($"https://localhost:5207/chat")
            .WithAutomaticReconnect(new SignalRRetryPolicy())
            .Build();

        RegisterHandlers();

        try
        {
            await _hubConnection.StartAsync();
            logger.LogError("Connection Status: {status}, {context}", "Connected to Server", User);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task SendMessage(string user, string message)
    {
        if (_hubConnection is not null && _hubConnection?.State == HubConnectionState.Connected)
        {
            logger.LogInformation($"ChatService.SendMessage() - {user}: {message}");
            await _hubConnection.SendAsync("SendMessage", user, message);
        }
    }

    private void RegisterHandlers()
    {
        #region ConnectionHandlers

        _hubConnection.Reconnecting += async error =>
        {
            OnStatusChanged?.Invoke(ChatStatus.Reconnecting);
            logger.LogError($"Reconnecting - {error}");
            await Task.CompletedTask;
        };

        _hubConnection.Reconnected += async error =>
        {
            OnStatusChanged?.Invoke(ChatStatus.Connected);
            logger.LogError($"Reconnected - {error}");
            await Task.CompletedTask;
        };

        _hubConnection.Closed += async error =>
        {
            OnStatusChanged?.Invoke(ChatStatus.NotConnected);
            logger.LogError($"Disconnected - {error}");
            await Task.CompletedTask;
        };

        #endregion
        #region MessageHandlers
        
        _hubConnection.On<string, string>("OnReceiveMessage", (user, message) =>
        {
            Console.WriteLine("OnReceiveMessage");
            if (OnMessageReceived is null) Console.WriteLine("OnMessageReceived is null");
            OnMessageReceived?.Invoke(user, message);
            logger.LogInformation("Connection Status: {status}, {context}", "ReceiveMessage", $"{user}: {message}");
        });
        
        _hubConnection.On<string>("ConnectionRejected", (messsage) =>
        {
            logger.LogInformation("Connection Status: {status}, {context}", "ConnectionRejected", messsage);
        });
        
        _hubConnection.On<string>("OnConnected", (user) =>
        {
            User = user;
            OnStatusChanged?.Invoke(ChatStatus.Connected);
            logger.LogInformation("Connection Status: {status}, {context}", "OnConnected", $"Connected as {user}");
        });

        #endregion
    }

    public async Task<bool> CheckUserExists(string user)
    {
        return await _hubConnection.InvokeAsync<bool>("CheckUserExists", user);
    }
}
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using StreamingLlmBlazor.Models;

namespace StreamingLlmBlazor.Components.Pages;
public partial class Home
{
    public string Input { get; set; } = string.Empty;
    private List<ChatMessage> Messages = new();
    private ClientWebSocket? _socket;

    [Inject] private NavigationManager Nav { get; set; } = default!;

    private async Task Generate()
    {
        if (string.IsNullOrWhiteSpace(Input))
            return;

        var userMessage = Input;
        Input = string.Empty;

        Messages.Add(new ChatMessage
        {
            Role = "user",
            Content = userMessage
        });

        await InvokeAsync(StateHasChanged);

        _socket = new ClientWebSocket();
        var wsUrl = Nav.ToAbsoluteUri("wss://localhost:7189/ws/chat");
        await _socket.ConnectAsync(wsUrl, CancellationToken.None);

        var sendBytes = Encoding.UTF8.GetBytes(userMessage);
        await _socket.SendAsync(sendBytes, WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);

        var botMsg = new ChatMessage
        {
            Role = "bot",
            Content = string.Empty
        };
        Messages.Add(botMsg);

        var buffer = new byte[4096];
        while (_socket.State == WebSocketState.Open)
        {
            var result = await _socket.ReceiveAsync(buffer, CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
                break;

            var text = Encoding.UTF8.GetString(buffer, 0, result.Count);

            botMsg.Content += text;

            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleKey(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
            await Generate();
    }
}
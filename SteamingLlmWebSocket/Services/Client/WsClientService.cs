using System.Net.WebSockets;

namespace StreamingLlmWebSocket.Services.Client;

public class WsClientService
{
    public async Task ForwardAsync(WebSocket wsBackend, WebSocket wsClient, CancellationToken ct)
    {
        var buffer = new byte[8192];

        while (wsBackend.State == WebSocketState.Open && wsClient.State == WebSocketState.Open)
        {
            var result = await wsClient.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await wsBackend.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Client closed",
                    ct);
                break;
            }

            await wsBackend.SendAsync(
                buffer.AsMemory(0, result.Count),
                result.MessageType,
                result.EndOfMessage,
                ct);
        }
    }

}

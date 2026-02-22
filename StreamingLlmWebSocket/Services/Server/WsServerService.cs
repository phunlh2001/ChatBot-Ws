using System.Net.WebSockets;

namespace StreamingLlmWebSocket.Services.Server;

public class WsServerService(IConfiguration config)
{
    private readonly IConfiguration _config = config;

    public async Task<ClientWebSocket> ConnectToServerAsync(CancellationToken ct)
    {
        var ws = new ClientWebSocket();
        var url = _config["BackendWsUrl"];

        await ws.ConnectAsync(new Uri(url!), ct);
        return ws;
    }

    public async Task BackwardAsync(WebSocket wsBackend, WebSocket wsClient, CancellationToken ct)
    {
        var buffer = new byte[8192];

        while (wsBackend.State == WebSocketState.Open && wsClient.State == WebSocketState.Open)
        {
            var result = await wsBackend.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await wsClient.CloseAsync(
                    closeStatus: WebSocketCloseStatus.NormalClosure,
                    statusDescription: "Backend close",
                    cancellationToken: ct);

                break;
            }

            await wsClient.SendAsync(
                buffer.AsMemory(0, result.Count),
                result.MessageType,
                result.EndOfMessage,
                ct);
        }
    }
}

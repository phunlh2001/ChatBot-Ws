using FastEndpoints;
using StreamingLlmWebSocket.Services.Client;
using StreamingLlmWebSocket.Services.Server;

namespace StreamingLlmWebSocket.Streaming;

public sealed class WsHandler(
    WsClientService client,
    WsServerService server
) : EndpointWithoutRequest
{
    private readonly WsServerService _serverService = server;
    private readonly WsClientService _clientService = client;

    public override void Configure()
    {
        Get("/ws/chat");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using var wsClient = await HttpContext.WebSockets.AcceptWebSocketAsync();
        using var wsBackend = await _serverService.ConnectToServerAsync(ct);

        await Task.WhenAny(
            _clientService.ForwardAsync(wsBackend, wsClient, ct),
            _serverService.BackwardAsync(wsBackend, wsClient, ct)
        );
    }
}

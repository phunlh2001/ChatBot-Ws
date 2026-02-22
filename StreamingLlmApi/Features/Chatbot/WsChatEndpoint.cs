using System.Net.WebSockets;
using System.Text;
using FastEndpoints;
using StramingLlmApi.Services;

namespace StramingLlmApi.Features.Chatbot;

public class WsChatEndpoint(AiChatService chatService) : EndpointWithoutRequest
{
    private readonly AiChatService _chatService = chatService;

    public override void Configure()
    {
        Verbs(Http.GET);
        Routes("/ws/chat");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        var buffer = new byte[8192];

        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(buffer, ct);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(
                    closeStatus: WebSocketCloseStatus.NormalClosure,
                    statusDescription: "Closed",
                    cancellationToken: ct);

                break;
            }

            var userMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

            await foreach (var chunk in _chatService.StreamingAnswerAsync(userMessage, ct))
            {
                var chunkBuffer = Encoding.UTF8.GetBytes(chunk);
                await socket.SendAsync(chunkBuffer, WebSocketMessageType.Text, endOfMessage: true, ct);
            }
        }
    }
}

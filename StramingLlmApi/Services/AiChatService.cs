using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using OpenAI.Chat;

namespace StramingLlmApi.Services;

public class AiChatService
{
    private readonly ChatClient _client;

    private readonly List<ChatMessage> _messages = new()
    {
        new SystemChatMessage("You are a helpful assistant.")
    };

    public AiChatService(IConfiguration config)
    {
        _client = new ChatClient("gpt-5", new ApiKeyCredential(config["OpenAI:ApiKey"]!));
    }

    public async IAsyncEnumerable<string> StreamingAnswerAsync(string userContent, [EnumeratorCancellation] CancellationToken ct = default)
    {
        _messages.Add(new UserChatMessage(userContent));
        var assistantSb = new StringBuilder();

        await foreach (var update in _client.CompleteChatStreamingAsync(_messages, cancellationToken: ct))
        {
            if (update.ContentUpdate is null)
                continue;

            foreach (var part in update.ContentUpdate)
                if (part is ChatMessageContentPart content)
                {
                    assistantSb.Append(content.Text);
                    yield return content.Text;
                }
        }

        _messages.Add(new AssistantChatMessage(assistantSb.ToString()));
    }
}

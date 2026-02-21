using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;
using OpenAI.Chat;

namespace StramingLlmApi.Services;

public class AiChatService(IConfiguration config)
{
    private readonly ChatClient _client = new(
        model: "gpt-5",
        credential: new ApiKeyCredential(config["OpenAI:ApiKey"]!)
    );

    private readonly List<ChatMessage> _messages = new()
    {
        new SystemChatMessage("You are a helpful assistant.")
    };

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

// See https://aka.ms/new-console-template for more information
using System.Text;
using OpenAI.Chat;
using StreamingLlmConsole;

var client = new ChatClient("gpt-4o", Constants.OpenAiKey);

Console.WriteLine("=======ENTER TO EXIT=======");
Console.WriteLine();

var messages = new List<ChatMessage>();

while (true)
{
    var sb = new StringBuilder();
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrEmpty(input))
        break;

    messages.Add(new UserChatMessage(input));

    Console.WriteLine();
    Console.Write("AI: ");

    var stream = client.CompleteChatStreamingAsync(messages);

    await foreach (var update in stream)
    {
        foreach (var content in update.ContentUpdate)
        {
            Console.Write(content.Text);
            sb.Append(content.Text);
        }
    }

    messages.Add(new AssistantChatMessage(sb.ToString()));

    Console.WriteLine();
    Console.WriteLine();
}
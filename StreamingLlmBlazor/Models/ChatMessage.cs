namespace StreamingLlmBlazor.Models;

public class ChatMessage
{
    public string Role { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
}

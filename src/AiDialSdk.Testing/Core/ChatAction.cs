namespace AiDialSdk.Testing.Core;

public class ChatAction
{
    public ChatAction(string reason, string description)
    {
        Reason = reason;
        Description = description;
    }

    public string Reason { get; }
    
    public string Description { get; }
}
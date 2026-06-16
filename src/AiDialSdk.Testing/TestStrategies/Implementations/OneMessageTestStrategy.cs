using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Data;
using AiDialSdk.Api.Files;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.TestStrategies.Models;

namespace AiDialSdk.Testing.TestStrategies.Implementations;

public class OneMessageTestStrategy : ITestStrategy
{
    private readonly string _message;
    private readonly IReadOnlyList<DialAttachment>? _attachments;
    
    public OneMessageTestStrategy(string message, IReadOnlyList<DialAttachment>? attachments = null)
    {
        _message = message;
        _attachments = attachments;
    }

    public async Task<TestResult> RunAsync(IDialChatApiClient chatClient, IDialFileApiClient _, CancellationToken token)
    {
        UserMessageCustomContent? customContent = null;
        if (_attachments != null)
        {
            customContent = new UserMessageCustomContent(null, _attachments);
        }
        var chatHistory = new List<BaseMessage> { new DialUserMessage(_message, customContent: customContent) };
        var chatResponse = await chatClient.CompleteChatAsync(chatHistory, new DialChatOptions(), token);
        
        chatHistory.Add(chatResponse.Message);
        
        return new TestResult(
            chatHistory,
            ["Completed after one message."], 
            chatResponse.Usage ?? new Usage(0,0,0));
    }
}
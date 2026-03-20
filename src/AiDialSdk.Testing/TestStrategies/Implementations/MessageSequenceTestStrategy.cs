using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.TestStrategies.Models;

namespace AiDialSdk.Testing.TestStrategies.Implementations;

public class MessageSequenceTestStrategy : ITestStrategy
{
    private readonly IReadOnlyList<BaseMessage> _chatHistory;

    public MessageSequenceTestStrategy(IReadOnlyList<BaseMessage> messages)
    {
        _chatHistory = messages;
    }
    
    public async Task<TestResult> RunAsync(IDialChatApiClient chatClient, CancellationToken token)
    {
        var chatHistory = new List<BaseMessage>(_chatHistory);
        var chatResponse = await chatClient.CompleteChatAsync(chatHistory, new DialChatOptions(), token);

        chatHistory.Add(chatResponse.Message);
        
        return new TestResult(
            chatHistory,
            ["Completed after message sequence."],
            chatResponse.Usage ?? new Usage(0,0,0));
    }
}
using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Extensions;
using AiDialSdk.Testing.Helpers;
using AiDialSdk.Testing.TestStrategies.Models;

namespace AiDialSdk.Testing.NUnit.Extensions;

public static class TestResultExtensions
{
    public static void WriteTo(this TestResult testResult, TextWriter writer, string agentName)
    {
        foreach (var completeReason in testResult.CompleteReasons)
        {
            writer.WriteLine($"Complete reason: {completeReason}");
        }
        
        writer.WriteLine("Conversation History:");
        
        testResult.Messages.WriteTo(writer, agentName);
    }

    private static void WriteTo(this IReadOnlyList<BaseMessage> messages, TextWriter writer, string agentName)
    {
        foreach (var message in messages)
        {
            switch (message)
            {
                case SystemMessage systemMessage:
                    writer.WriteLine($"System message [{agentName}]: {systemMessage.Content.Trim()}");
                    break;
                case DialUserMessage userMessage:
                    writer.WriteLine($"User message: {userMessage.Content.Trim()}");
                    break;
                case DialAssistantMessage assistantMessage:
                    assistantMessage.GetToolExecutionHistory().WriteTo(writer, agentName);
                
                    if (!string.IsNullOrWhiteSpace(assistantMessage.Content))
                        writer.WriteLine($"Assistant message [{agentName}]: {assistantMessage.Content.Trim()}");
                    break;
                case DialToolMessage toolMessage:
                {
                    var toolCall = messages
                        .OfType<DialAssistantMessage>()
                        .SelectMany(m => m.ToolCalls?.Where(tc => tc.Id == toolMessage.ToolCallId) ?? [])
                        .Single();
                    
                    var toolName = ToolHelpers.SanitizeToolName(toolCall.Function.Name);
                    
                    writer.WriteLine($"Tool call [{agentName}]: '{toolName}' called with arguments: '{toolCall.Function.Arguments}'");
                    toolMessage.GetToolExecutionHistory().WriteTo(writer, toolName);
                    writer.WriteLine($"Tool message [{toolName}]: {toolMessage.Content.Trim()}");
                }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown message type: {message.GetType().Name}");
            }
        }
    }
}
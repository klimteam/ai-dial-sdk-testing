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
        
        writer.WriteLine($"Total message count: {testResult.Messages.Count}");
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
                    writer.WriteLine($"[{agentName}][System message]: {systemMessage.Content.Trim()}");
                    break;
                case DialUserMessage userMessage:
                    writer.WriteLine($"[User][User message]: {userMessage.Content.Trim()}");
                    break;
                case DialAssistantMessage assistantMessage:
                    assistantMessage.GetToolExecutionHistory().WriteTo(writer, agentName);
                
                    if (!string.IsNullOrWhiteSpace(assistantMessage.Content))
                        writer.WriteLine($"[{agentName}][Assistant message]: {assistantMessage.Content.Trim()}");
                    break;
                case DialToolMessage toolMessage:
                {
                    var toolCall = messages
                        .OfType<DialAssistantMessage>()
                        .SelectMany(m => m.ToolCalls?.Where(tc => tc.Id == toolMessage.ToolCallId) ?? [])
                        .Single();
                    
                    var toolName = ToolHelpers.SanitizeToolName(toolCall.Function.Name);
                    
                    if (Constants.QuickApps.SystemToolNames.Contains(toolName, StringComparer.OrdinalIgnoreCase))
                        continue;
                    
                    writer.WriteLine($"[{agentName}][Tool call]: '{toolName}' called with arguments: '{toolCall.Function.Arguments}'");
                    toolMessage.GetToolExecutionHistory().WriteTo(writer, toolName);
                    writer.WriteLine($"[{toolName}][Tool message]: {toolMessage.Content.Trim()}");
                }
                    break;
                default:
                    throw new InvalidOperationException($"Unknown message type: {message.GetType().Name}");
            }
        }
    }
}
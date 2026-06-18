namespace AiDialSdk.Testing;

public static class Constants
{
    public const string ToolExecutionHistoryKey = "tool_execution_history";

    public const string SendNextMessageToChatToolName = "send_next_message_to_chat";
    public const string MarkTestAsSuccessfulToolName  = "mark_test_as_successful";
    public const string MarkTestAsFailedToolName  = "mark_test_as_failed";
    
    public static class QuickApps
    {
        private const string ReadSkillToolName = "internal_skills_read_skill";
        private const string TimeAwarenessCurrentTimestamp = "internal_timeawareness_current_timestamp";
        private const string FileParameterFormattingToolName = "tool-call-file-parameter-formatting";
        
        public static readonly string[] SystemToolNames =
        [
            ReadSkillToolName,
            TimeAwarenessCurrentTimestamp,
            FileParameterFormattingToolName
        ];
    }
}
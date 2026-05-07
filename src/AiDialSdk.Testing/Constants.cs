namespace AiDialSdk.Testing;

public static class Constants
{
    public const string ToolExecutionHistoryKey = "tool_execution_history";
    
    public static class QuickApps
    {
        private const string ReadSkillToolName = "internal_skills_read_skill";
        private const string FileParameterFormattingToolName = "tool-call-file-parameter-formatting";
        
        public static readonly string[] SystemToolNames =
        [
            ReadSkillToolName,
            FileParameterFormattingToolName
        ];
    }
}
namespace AiDialSdk.Testing.Helpers;

public static class ToolHelpers
{
    public static string SanitizeToolName(string toolName)
    {
        return toolName.ToLower();
        //return toolName.Trim()[..(toolName.Length - 5)].ToLowerInvariant();
    }
}
namespace AiDialSdk.Testing.Core;

public abstract record ChatActionResult;

public record AppendMessageContentChatActionResult(string ContentToAppend) : ChatActionResult;

public record ReplaceMessageContentChatActionResult(string NewContent) : ChatActionResult;

public record MockMessageChatActionResult(string Content) : ChatActionResult;
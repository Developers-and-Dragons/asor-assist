namespace AsorAssistant.Domain.Models;

public static class ExecutionMode
{
    public const string Ambient = "Mode=Ambient";
    public const string Delegate = "Mode=Delegate";

    public static bool IsValid(string? value) =>
        value is Ambient or Delegate;
}

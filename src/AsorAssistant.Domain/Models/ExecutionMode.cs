namespace AsorAssistant.Domain.Models;

public class ExecutionMode
{
    public string? Id { get; set; }
    public string? Descriptor { get; set; }

    public const string Ambient = "Mode=Ambient";
    public const string Delegate = "Mode=Delegate";

    public static bool IsValidId(string? value) =>
        value is Ambient or Delegate;
}

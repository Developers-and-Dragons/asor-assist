namespace AsorAssistant.App.ViewModels;

public class LookupOption
{
    public required string Name { get; init; }
    public required string Id { get; init; }

    public override string ToString() => Name;
}

namespace AsorAssistant.Core.Models;

public class ServiceOperationLookup
{
    public required string Name { get; init; }
    public required string WorkdayId { get; init; }
    public string? ServiceName { get; init; }
    public required ServiceOperationType Type { get; init; }
}

public enum ServiceOperationType
{
    Soap,
    Rest
}

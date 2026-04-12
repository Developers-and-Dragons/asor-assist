namespace AsorAssistant.Application.Models;

public class DraftMetadata
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public string? Provider { get; set; }
    public string? Version { get; set; }
    public string? WorkdayHost { get; set; }
    public string? TenantName { get; set; }
    public DateTimeOffset LastModified { get; set; }
}

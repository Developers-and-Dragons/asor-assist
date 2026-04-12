using System.Text.Json.Serialization;

namespace AsorAssistant.Domain.Models;

public class Platform
{
    public string? Id { get; set; }
    public string? Descriptor { get; set; }

    [JsonPropertyName("reference_id")]
    public string? ReferenceId { get; set; }
}

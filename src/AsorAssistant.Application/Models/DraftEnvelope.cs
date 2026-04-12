using AsorAssistant.Domain.Models;

namespace AsorAssistant.Application.Models;

public class DraftEnvelope
{
    public required DraftMetadata Metadata { get; set; }
    public required AgentDefinition Definition { get; set; }
}

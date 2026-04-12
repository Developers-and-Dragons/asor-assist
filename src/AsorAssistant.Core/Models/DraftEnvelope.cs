using AsorAssistant.Domain.Models;

namespace AsorAssistant.Core.Models;

public class DraftEnvelope
{
    public required DraftMetadata Metadata { get; set; }
    public required AgentDefinition Definition { get; set; }
}

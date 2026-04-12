using AsorAssistant.Core.Models;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Core.Ports;

public interface IAsorQueryClient
{
    Task<AgentDefinition?> GetDefinitionAsync(RegistrationContext context, string id);
}

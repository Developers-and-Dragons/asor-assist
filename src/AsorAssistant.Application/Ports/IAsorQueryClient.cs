using AsorAssistant.Application.Models;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Application.Ports;

public interface IAsorQueryClient
{
    Task<AgentDefinition?> GetDefinitionAsync(RegistrationContext context, string id);
}

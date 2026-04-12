using AsorAssistant.Core.Models;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Core.Ports;

public interface IAsorRegistrationClient
{
    Task<RegistrationResult> RegisterAsync(RegistrationContext context, AgentDefinition definition);
}

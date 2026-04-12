using AsorAssistant.Application.Models;
using AsorAssistant.Domain.Models;

namespace AsorAssistant.Application.Ports;

public interface IAsorRegistrationClient
{
    Task<RegistrationResult> RegisterAsync(RegistrationContext context, AgentDefinition definition);
}

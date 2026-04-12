using AsorAssistant.Domain.Models;

namespace AsorAssistant.Core.Models;

public class RegistrationContext
{
    public AsorRegion? Region { get; set; }
    public string? TenantName { get; set; }
    public string? BearerToken { get; set; }
}

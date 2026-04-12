using AsorAssistant.Domain.Models;

namespace AsorAssistant.Application.Models;

public class RegistrationContext
{
    public AsorRegion? Region { get; set; }
    public string? TenantName { get; set; }
    public string? BearerToken { get; set; }
}

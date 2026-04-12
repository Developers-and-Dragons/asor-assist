namespace AsorAssistant.Domain.Models;

public class Capabilities
{
    public bool? PushNotifications { get; set; }
    public bool? Streaming { get; set; }
    public bool? StateTransitionHistory { get; set; }
}

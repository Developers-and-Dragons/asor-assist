namespace AsorAssistant.Domain.Models;

public class AsorRegion
{
    public required string Name { get; init; }
    public required string BaseUrl { get; init; }

    public static IReadOnlyList<AsorRegion> All { get; } =
    [
        new() { Name = "US", BaseUrl = "https://us.agent.workday.com" },
        new() { Name = "EU", BaseUrl = "https://eu.agent.workday.com" },
        new() { Name = "UK", BaseUrl = "https://uk.agent.workday.com" },
        new() { Name = "SIN", BaseUrl = "https://sg.agent.workday.com" },
        new() { Name = "IND", BaseUrl = "https://in.agent.workday.com" },
        new() { Name = "JPN", BaseUrl = "https://jp.agent.workday.com" },
    ];
}

using System.Collections.ObjectModel;
using AsorAssistant.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AsorAssistant.App.ViewModels;

public partial class SkillViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _id;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _tagsText;

    [ObservableProperty]
    private string? _inputModesText;

    [ObservableProperty]
    private string? _outputModesText;

    public AgentSkill ToModel() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Tags = ParseTags(TagsText),
        InputModes = ParseCommaSeparated(InputModesText),
        OutputModes = ParseCommaSeparated(OutputModesText)
    };

    public static SkillViewModel FromModel(AgentSkill skill) => new()
    {
        Id = skill.Id,
        Name = skill.Name,
        Description = skill.Description,
        TagsText = skill.Tags is not null
            ? string.Join(", ", skill.Tags.Where(t => t.Tag is not null).Select(t => t.Tag))
            : null,
        InputModesText = skill.InputModes is not null ? string.Join(", ", skill.InputModes) : null,
        OutputModesText = skill.OutputModes is not null ? string.Join(", ", skill.OutputModes) : null
    };

    private static List<SkillTag>? ParseTags(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => new SkillTag { Tag = t })
            .ToList();
    }

    private static List<string>? ParseCommaSeparated(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}

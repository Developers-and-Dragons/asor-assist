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

    // Inline validation errors
    [ObservableProperty]
    private string? _idError;

    [ObservableProperty]
    private string? _nameError;

    [ObservableProperty]
    private string? _descriptionError;

    public void ClearErrors()
    {
        IdError = null;
        NameError = null;
        DescriptionError = null;
    }

    public AgentSkill ToModel() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Tags = ParseTags(TagsText),
        InputModes = ParseMimeTypes(InputModesText),
        OutputModes = ParseMimeTypes(OutputModesText)
    };

    public static SkillViewModel FromModel(AgentSkill skill) => new()
    {
        Id = skill.Id,
        Name = skill.Name,
        Description = skill.Description,
        TagsText = skill.Tags is not null
            ? string.Join(", ", skill.Tags.Where(t => t.Tag is not null).Select(t => t.Tag))
            : null,
        InputModesText = skill.InputModes is not null
            ? string.Join(", ", skill.InputModes.Where(m => m.Type is not null).Select(m => m.Type))
            : null,
        OutputModesText = skill.OutputModes is not null
            ? string.Join(", ", skill.OutputModes.Where(m => m.Type is not null).Select(m => m.Type))
            : null
    };

    private static List<SkillTag>? ParseTags(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => new SkillTag { Tag = t })
            .ToList();
    }

    internal static List<MimeType>? ParseMimeTypes(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => new MimeType { Type = t })
            .ToList();
    }
}

using System.Collections.ObjectModel;
using AsorAssistant.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    // --- Workday Config (opt-in per skill) ---

    [ObservableProperty]
    private bool _hasWorkdayConfig;

    [ObservableProperty]
    private LookupOption? _selectedExecutionModeOption;

    public ObservableCollection<WorkdayResourceViewModel> WorkdayResources { get; } = [];

    public static IReadOnlyList<LookupOption> ExecutionModeOptions { get; } =
    [
        new LookupOption { Name = "Delegate", Id = ExecutionMode.Delegate },
        new LookupOption { Name = "Ambient", Id = ExecutionMode.Ambient },
    ];

    [RelayCommand]
    private void AddWorkdayResource()
    {
        WorkdayResources.Add(new WorkdayResourceViewModel());
    }

    [RelayCommand]
    private void RemoveWorkdayResource(WorkdayResourceViewModel resource)
    {
        WorkdayResources.Remove(resource);
    }

    [RelayCommand]
    private void EnableWorkdayConfig()
    {
        HasWorkdayConfig = true;
        if (WorkdayResources.Count == 0)
            WorkdayResources.Add(new WorkdayResourceViewModel());
    }

    [RelayCommand]
    private void RemoveWorkdayConfig()
    {
        HasWorkdayConfig = false;
        SelectedExecutionModeOption = null;
        WorkdayResources.Clear();
    }

    // --- Inline validation errors ---

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

    partial void OnIdChanged(string? value) => IdError = null;
    partial void OnNameChanged(string? value) => NameError = null;
    partial void OnDescriptionChanged(string? value) => DescriptionError = null;

    public AgentSkill ToModel() => new()
    {
        Id = Id,
        Name = Name,
        Description = Description,
        Tags = ParseTags(TagsText),
        InputModes = ParseMimeTypes(InputModesText),
        OutputModes = ParseMimeTypes(OutputModesText)
    };

    /// <summary>
    /// Builds an AgentSkillResource if this skill has Workday Config enabled, otherwise null.
    /// </summary>
    public AgentSkillResource? ToWorkdayConfigModel()
    {
        if (!HasWorkdayConfig) return null;

        return new AgentSkillResource
        {
            SkillId = Id,
            ExecutionMode = new ExecutionMode { Id = SelectedExecutionModeOption?.Id },
            WorkdayResources = WorkdayResources.Select(r => r.ToModel()).ToList()
        };
    }

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

    /// <summary>
    /// Apply WorkdayConfig data from a matching AgentSkillResource onto this skill VM.
    /// </summary>
    public void ApplyWorkdayConfig(AgentSkillResource resource)
    {
        HasWorkdayConfig = true;
        SelectedExecutionModeOption = MatchExecutionMode(resource.ExecutionMode);

        WorkdayResources.Clear();
        if (resource.WorkdayResources is not null)
        {
            foreach (var wr in resource.WorkdayResources)
                WorkdayResources.Add(WorkdayResourceViewModel.FromModel(wr));
        }
    }

    private static LookupOption? MatchExecutionMode(ExecutionMode? mode)
    {
        if (mode is null) return null;

        var byId = ExecutionModeOptions.FirstOrDefault(o => o.Id == mode.Id);
        if (byId is not null) return byId;

        var byDescriptor = ExecutionModeOptions.FirstOrDefault(o =>
            string.Equals(o.Name, mode.Descriptor, StringComparison.OrdinalIgnoreCase));
        if (byDescriptor is not null) return byDescriptor;

        return ExecutionModeOptions.FirstOrDefault(o =>
            mode.Id is not null && mode.Id.Contains(o.Name, StringComparison.OrdinalIgnoreCase));
    }

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

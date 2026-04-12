using System.Collections.ObjectModel;
using AsorAssistant.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class AgentSkillResourceViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _skillId;

    [ObservableProperty]
    private LookupOption? _selectedExecutionModeOption;

    public ObservableCollection<WorkdayResourceViewModel> WorkdayResources { get; } = [];

    /// <summary>Reference to the editor's skills collection for the skill ID dropdown.</summary>
    public ObservableCollection<SkillViewModel>? AvailableSkills { get; set; }

    public static IReadOnlyList<LookupOption> ExecutionModeOptions { get; } =
    [
        new LookupOption { Name = "Ambient", Id = ExecutionMode.Ambient },
        new LookupOption { Name = "Delegate", Id = ExecutionMode.Delegate },
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

    public AgentSkillResource ToModel() => new()
    {
        SkillId = SkillId,
        ExecutionMode = new ExecutionMode { Id = SelectedExecutionModeOption?.Id },
        WorkdayResources = WorkdayResources.Select(r => r.ToModel()).ToList()
    };

    public static AgentSkillResourceViewModel FromModel(AgentSkillResource resource, ObservableCollection<SkillViewModel>? availableSkills = null)
    {
        var vm = new AgentSkillResourceViewModel
        {
            AvailableSkills = availableSkills,
            SkillId = resource.SkillId,
            SelectedExecutionModeOption = ExecutionModeOptions.FirstOrDefault(o => o.Id == resource.ExecutionMode?.Id)
        };

        if (resource.WorkdayResources is not null)
        {
            foreach (var wr in resource.WorkdayResources)
                vm.WorkdayResources.Add(WorkdayResourceViewModel.FromModel(wr));
        }

        return vm;
    }
}

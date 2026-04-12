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
    private string? _selectedExecutionMode;

    public ObservableCollection<WorkdayResourceViewModel> WorkdayResources { get; } = [];

    /// <summary>Reference to the editor's skills collection for the skill ID dropdown.</summary>
    public ObservableCollection<SkillViewModel>? AvailableSkills { get; set; }

    public static IReadOnlyList<string> ExecutionModeOptions { get; } =
        [ExecutionMode.Ambient, ExecutionMode.Delegate];

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
        ExecutionMode = new ExecutionMode { Id = SelectedExecutionMode },
        WorkdayResources = WorkdayResources.Select(r => r.ToModel()).ToList()
    };

    public static AgentSkillResourceViewModel FromModel(AgentSkillResource resource, ObservableCollection<SkillViewModel>? availableSkills = null)
    {
        var vm = new AgentSkillResourceViewModel
        {
            AvailableSkills = availableSkills,
            SkillId = resource.SkillId,
            SelectedExecutionMode = resource.ExecutionMode?.Id
        };

        if (resource.WorkdayResources is not null)
        {
            foreach (var wr in resource.WorkdayResources)
                vm.WorkdayResources.Add(WorkdayResourceViewModel.FromModel(wr));
        }

        return vm;
    }
}

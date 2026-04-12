using System.Collections.ObjectModel;
using AsorAssistant.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AsorAssistant.App.ViewModels;

public partial class SecurableItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _id;
}

public partial class WorkdayResourceViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _toolName;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _agentResourceId;

    public ObservableCollection<SecurableItemViewModel> Tools { get; } = [];

    [RelayCommand]
    private void AddTool()
    {
        Tools.Add(new SecurableItemViewModel());
    }

    [RelayCommand]
    private void RemoveTool(SecurableItemViewModel tool)
    {
        Tools.Remove(tool);
    }

    public WorkdayResource ToModel() => new()
    {
        ToolName = ToolName,
        Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
        AgentResource = string.IsNullOrWhiteSpace(AgentResourceId) ? null : new AgentResource { Id = AgentResourceId },
        Tools = Tools.Count > 0
            ? Tools.Select(t => new SecurableItem { Id = t.Id }).ToList()
            : null
    };

    public static WorkdayResourceViewModel FromModel(WorkdayResource resource) => FromModelCore(resource);

    private static WorkdayResourceViewModel FromModelCore(WorkdayResource resource)
    {
        var vm = new WorkdayResourceViewModel
        {
            ToolName = resource.ToolName,
            Description = resource.Description,
            AgentResourceId = resource.AgentResource?.Id
        };

        if (resource.Tools is not null)
        {
            foreach (var tool in resource.Tools)
                vm.Tools.Add(new SecurableItemViewModel { Id = tool.Id });
        }

        return vm;
    }
}

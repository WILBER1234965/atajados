using atajados.Models;
using CommunityToolkit.Mvvm.Input;

namespace atajados.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}
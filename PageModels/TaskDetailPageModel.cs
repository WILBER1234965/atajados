using atajados.Data;
using atajados.Models;
using atajados.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace atajados.PageModels
{
    public partial class TaskDetailPageModel : ObservableObject, IQueryAttributable
    {
        public const string ProjectQueryKey = "project";
        private ProjectTask? _task;
        private bool _canDelete;
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private bool _isCompleted;

        [ObservableProperty]
        private List<Project> _projects = [];

        [ObservableProperty]
        private Project? _project;

        [ObservableProperty]
        private int _selectedProjectIndex = -1;

        [ObservableProperty]
        private bool _isExistingProject;

        public TaskDetailPageModel(
            ProjectRepository projectRepository,
            TaskRepository taskRepository,
            ModalErrorHandler errorHandler)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _errorHandler = errorHandler;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            LoadTaskAsync(query).FireAndForgetSafeAsync(_errorHandler);
        }

        private async Task LoadTaskAsync(IDictionary<string, object> query)
        {
            if (query.TryGetValue(ProjectQueryKey, out var project))
                Project = (Project)project;

            int taskId = 0;

            if (query.ContainsKey("id"))
            {
                taskId = Convert.ToInt32(query["id"]);
                _task = await _taskRepository.GetAsync(taskId);

                if (_task is null)
                {
                    _errorHandler.HandleError(new Exception($"Task Id {taskId} isn't valid."));
                    return;
                }

                Project = await _projectRepository.GetAsync(_task.ProjectID);
            }
            else
            {
                _task = new ProjectTask();
            }

            if (Project?.ID == 0)
            {
                IsExistingProject = false;
            }
            else
            {
                Projects = await _projectRepository.ListAsync();
                IsExistingProject = true;
            }

            if (Project is not null)
                SelectedProjectIndex = Projects.FindIndex(p => p.ID == Project.ID);
            else if (_task?.ProjectID > 0)
                SelectedProjectIndex = Projects.FindIndex(p => p.ID == _task.ProjectID);

            if (taskId > 0)
            {
                Title = _task.Title;
                IsCompleted = _task.IsCompleted;
                CanDelete = true;
            }
            else
            {
                _task = new ProjectTask() { ProjectID = Project?.ID ?? 0 };
            }
        }

        public bool CanDelete
        {
            get => _canDelete;
            set
            {
                _canDelete = value;
                DeleteCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (_task is null)
            {
                _errorHandler.HandleError(
                    new Exception("Task or project is null. The task could not be saved."));
                return;
            }

            _task.Title = Title;
            int projectId = Project?.ID ?? 0;

            if (Projects.Count > SelectedProjectIndex && SelectedProjectIndex >= 0)
                projectId = Projects[SelectedProjectIndex].ID;

            _task.ProjectID = projectId;
            _task.IsCompleted = IsCompleted;

            if (Project?.ID == projectId && !Project.Tasks.Contains(_task))
                Project.Tasks.Add(_task);

            if (_task.ID > 0)
                await _taskRepository.SaveItemAsync(_task);

            await Shell.Current.GoToAsync("..?refresh=true");

            if (_task.ID > 0)
            {
                var toast = Toast.Make("Task saved", ToastDuration.Short);
                await toast.Show();
            }
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        private async Task Delete()
        {
            if (_task is null || Project is null)
            {
                _errorHandler.HandleError(
                    new Exception("Task is null. The task could not be deleted."));
                return;
            }

            if (Project.Tasks.Contains(_task))
                Project.Tasks.Remove(_task);

            if (_task.ID > 0)
                await _taskRepository.DeleteItemAsync(_task);

            await Shell.Current.GoToAsync("..?refresh=true");

            var toast = Toast.Make("Task deleted", ToastDuration.Short);
            await toast.Show();
        }
    }
}
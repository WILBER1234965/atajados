using atajados.Models;
using atajados.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace atajados.PageModels
{
    public partial class ProjectDetailPageModel : ObservableObject, IQueryAttributable, IProjectTaskPageModel
    {
        private Project? _project;
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly TagRepository _tagRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private List<ProjectTask> _tasks = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private List<Category> _categories = new();

        [ObservableProperty]
        private int _categoryIndex;

        [ObservableProperty]
        private List<Tag> _allTags = new();

        [ObservableProperty]
        private IconData _icon = new IconData();

        public bool HasCompletedTasks => _project?.Tasks.Any(t => t.IsCompleted) ?? false;

        public ProjectDetailPageModel(
            ProjectRepository projectRepository,
            TaskRepository taskRepository,
            CategoryRepository categoryRepository,
            TagRepository tagRepository,
            ModalErrorHandler errorHandler)
        {
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _errorHandler = errorHandler;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var idObj) && int.TryParse(idObj.ToString(), out var id))
            {
                LoadData(id).FireAndForgetSafeAsync(_errorHandler);
            }
            else if (query.ContainsKey("refresh"))
            {
                RefreshData().FireAndForgetSafeAsync(_errorHandler);
            }
            else
            {
                _project = new Project();
                Tasks = _project.Tasks;
                Task.WhenAll(LoadCategories(), LoadTags()).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        private async Task LoadData(int id)
        {
            try
            {
                IsBusy = true;
                _project = await _projectRepository.GetAsync(id) ?? new Project();
                Name = _project.Name;
                Description = _project.Description;
                Tasks = _project.Tasks;
                Icon.Icon = _project.Icon;

                Categories = await _categoryRepository.ListAsync();
                CategoryIndex = Categories.FindIndex(c => c.ID == _project.CategoryID);
                AllTags = await _tagRepository.ListAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(HasCompletedTasks));
            }
        }

        private async Task LoadCategories()
        {
            Categories = await _categoryRepository.ListAsync();
        }

        private async Task LoadTags()
        {
            AllTags = await _tagRepository.ListAsync();
        }

        private async Task RefreshData()
        {
            if (_project == null) return;
            Tasks = await _taskRepository.ListAsync(_project.ID);
            OnPropertyChanged(nameof(HasCompletedTasks));
        }

        [RelayCommand]
        private Task NavigateToTask(ProjectTask task)
            => Shell.Current.GoToAsync($"task?id={task.ID}");

        [RelayCommand]
        private async Task Save()
        {
            if (_project == null) return;
            IsBusy = true;

            _project.Name = Name;
            _project.Description = Description;
            _project.CategoryID = Categories.ElementAtOrDefault(CategoryIndex)?.ID ?? 0;
            _project.Icon = Icon.Icon;

            await _projectRepository.SaveItemAsync(_project);
            IsBusy = false;

            await Toast.Make("Project saved", ToastDuration.Short).Show();
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (_project == null) return;
            IsBusy = true;
            await _projectRepository.DeleteItemAsync(_project);
            IsBusy = false;

            await Toast.Make("Project deleted", ToastDuration.Short).Show();
        }

        [RelayCommand]
        private async Task CleanTasks()
        {
            var completed = Tasks.Where(t => t.IsCompleted).ToArray();
            foreach (var t in completed)
            {
                await _taskRepository.DeleteItemAsync(t);
                Tasks.Remove(t);
            }

            OnPropertyChanged(nameof(HasCompletedTasks));
            await Toast.Make("All cleaned up!", ToastDuration.Short).Show();
        }

        [RelayCommand]
        private async Task TaskCompleted(ProjectTask task)
        {
            await _taskRepository.SaveItemAsync(task);
            OnPropertyChanged(nameof(HasCompletedTasks));
        }
    }
}
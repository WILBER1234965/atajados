using atajados.Models;
using atajados.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace atajados.PageModels
{
    public partial class MainPageModel : ObservableObject, IProjectTaskPageModel
    {
        private bool _isNavigatedTo;
        private bool _dataLoaded;
        private readonly SeedDataService _seedDataService;
        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly ModalErrorHandler _errorHandler;

        [ObservableProperty]
        private ObservableCollection<CategoryChartData> _todoCategoryData = new();

        [ObservableProperty]
        private ObservableCollection<Brush> _todoCategoryColors = new();

        [ObservableProperty]
        private ObservableCollection<ProjectTask> _tasks = new();

        [ObservableProperty]
        private ObservableCollection<Project> _projects = new();

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _today = DateTime.Now.ToString("dddd, MMM d");

        public bool HasCompletedTasks => Tasks.Any(t => t.IsCompleted);

        public MainPageModel(
            SeedDataService seedDataService,
            ProjectRepository projectRepository,
            TaskRepository taskRepository,
            CategoryRepository categoryRepository,
            ModalErrorHandler errorHandler)
        {
            _seedDataService = seedDataService;
            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _categoryRepository = categoryRepository;
            _errorHandler = errorHandler;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsBusy = true;

                var projectsList = await _projectRepository.ListAsync();
                Projects = new ObservableCollection<Project>(projectsList);

                var categories = await _categoryRepository.ListAsync();
                var chartData = categories
                    .Select(c => new CategoryChartData(
                        c.Title,
                        Projects.Where(p => p.CategoryID == c.ID)
                                .SelectMany(p => p.Tasks)
                                .Count()))
                    .ToList();

                TodoCategoryData = new ObservableCollection<CategoryChartData>(chartData);
                TodoCategoryColors = new ObservableCollection<Brush>(
                    categories.Select(c => c.ColorBrush));

                var tasksList = await _taskRepository.ListAsync();
                Tasks = new ObservableCollection<ProjectTask>(tasksList);
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

        private async Task InitializeAsync()
        {
            if (!Preferences.Default.ContainsKey("is_seeded"))
            {
                await _seedDataService.LoadSeedDataAsync();
                Preferences.Default.Set("is_seeded", true);
            }
            await RefreshAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            IsRefreshing = true;
            await LoadDataAsync();
            IsRefreshing = false;
        }

        [RelayCommand]
        private async Task AppearingAsync()
        {
            if (!_dataLoaded)
            {
                await InitializeAsync();
                _dataLoaded = true;
            }
            else if (!_isNavigatedTo)
            {
                await RefreshAsync();
            }
        }

        [RelayCommand]
        private void NavigatedTo() => _isNavigatedTo = true;

        [RelayCommand]
        private void NavigatedFrom() => _isNavigatedTo = false;

        [RelayCommand]
        private Task TaskCompletedAsync(ProjectTask task)
        {
            OnPropertyChanged(nameof(HasCompletedTasks));
            return _taskRepository.SaveItemAsync(task);
        }

        [RelayCommand]
        private Task AddTaskAsync()
            => Shell.Current.GoToAsync("task");

        [RelayCommand]
        private Task NavigateToProjectAsync(Project project)
            => Shell.Current.GoToAsync($"project?id={project.ID}");

        [RelayCommand]
        private Task NavigateToTaskAsync(ProjectTask task)
            => Shell.Current.GoToAsync($"task?id={task.ID}");

        [RelayCommand]
        private async Task CleanTasksAsync()
        {
            var completedTasks = Tasks.Where(t => t.IsCompleted).ToList();
            foreach (var t in completedTasks)
            {
                await _taskRepository.DeleteItemAsync(t);
                Tasks.Remove(t);
            }
            OnPropertyChanged(nameof(HasCompletedTasks));
            await Toast.Make("All cleaned up!", ToastDuration.Short).Show();
        }
    }
}

using atajados.Data;
using atajados.Models;
using atajados.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace atajados.PageModels
{
    public partial class ManageMetaPageModel : ObservableObject
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly TagRepository _tagRepository;
        private readonly SeedDataService _seedDataService;

        [ObservableProperty]
        private ObservableCollection<Category> _categories = new();

        [ObservableProperty]
        private ObservableCollection<Tag> _tags = new();

        public ManageMetaPageModel(
            CategoryRepository categoryRepository,
            TagRepository tagRepository,
            SeedDataService seedDataService)
        {
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _seedDataService = seedDataService;
        }

        private async Task LoadData()
        {
            var categoriesList = await _categoryRepository.ListAsync();
            Categories = new ObservableCollection<Category>(categoriesList);
            var tagsList = await _tagRepository.ListAsync();
            Tags = new ObservableCollection<Tag>(tagsList);
        }

        [RelayCommand]
        private Task Appearing() => LoadData();

        [RelayCommand]
        private async Task SaveCategories()
        {
            foreach (var category in Categories)
            {
                await _categoryRepository.SaveItemAsync(category);
            }

            var toast = Toast.Make("Categories saved", ToastDuration.Short);
            await toast.Show();
        }

        [RelayCommand]
        private async Task DeleteCategory(Category category)
        {
            Categories.Remove(category);
            await _categoryRepository.DeleteItemAsync(category);
            var toast = Toast.Make("Category deleted", ToastDuration.Short);
            await toast.Show();
        }

        [RelayCommand]
        private async Task AddCategory()
        {
            var category = new Category();
            Categories.Add(category);
            await _categoryRepository.SaveItemAsync(category);
            var toast = Toast.Make("Category added", ToastDuration.Short);
            await toast.Show();
        }

        [RelayCommand]
        private async Task SaveTags()
        {
            foreach (var tag in Tags)
            {
                await _tagRepository.SaveItemAsync(tag);
            }

            var toast = Toast.Make("Tags saved", ToastDuration.Short);
            await toast.Show();
        }

        [RelayCommand]
        private async Task DeleteTag(Tag tag)
        {
            Tags.Remove(tag);
            await _tagRepository.DeleteItemAsync(tag);
            var toast = Toast.Make("Tag deleted", ToastDuration.Short);
            await toast.Show();
        }

        [RelayCommand]
        private async Task AddTag()
        {
            var tag = new Tag();
            Tags.Add(tag);
            await _tagRepository.SaveItemAsync(tag);
            var toast = Toast.Make("Tag added", ToastDuration.Short);
            await toast.Show();
        }

        [RelayCommand]
        private async Task Reset()
        {
            Preferences.Default.Remove("is_seeded");
            await _seedDataService.LoadSeedDataAsync();
            Preferences.Default.Set("is_seeded", true);
            await Shell.Current.GoToAsync("//main");
        }
    }
}

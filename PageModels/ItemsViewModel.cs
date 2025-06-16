using atajados.Models;
using atajados.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace atajados.PageModels
{
    public partial class ItemsViewModel : ObservableObject
    {
        private readonly DatabaseService _db;
        private readonly ExcelService _excel;

        public ObservableCollection<Item> Items { get; } = new();

        // Propiedad para el indicador de actividad (spinner)
        [ObservableProperty]
        private bool _isBusy;

        // --------------------------------------------------------------------
        //  Comandos expuestos a la interfaz (XAML)
        // --------------------------------------------------------------------
        public ICommand LoadItemsCommand => new AsyncRelayCommand(LoadItemsAsync);
        public ICommand ImportItemsCommand => new AsyncRelayCommand(ImportItemsAsync);
        public ICommand ExportItemsCommand => new AsyncRelayCommand(ExportItemsAsync);

        //  ⬇ Añadimos los tres comandos que antes chocaban con el generador
        public ICommand AddItemCommand => new AsyncRelayCommand(AddItemAsync);
        public ICommand DeleteItemCommand => new AsyncRelayCommand<Item>(DeleteItemAsync);
        public ICommand EditItemCommand => new AsyncRelayCommand<Item>(EditItemAsync);

        public ItemsViewModel(DatabaseService db, ExcelService excel)
        {
            _db = db;
            _excel = excel;

            // Carga inicial de la lista
            _ = LoadItemsAsync();
        }

        // --------------------------------------------------------------------
        //  Carga de ítems
        // --------------------------------------------------------------------
        private async Task LoadItemsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Items.Clear();
                var list = await _db.GetItemsAsync();
                foreach (var it in list)
                    Items.Add(it);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error cargando ítems", ex.Message, "OK");
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --------------------------------------------------------------------
        //  Importar desde Excel
        // --------------------------------------------------------------------
        private async Task ImportItemsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var file = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Selecciona archivo Excel",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI,   new[] { ".xlsx" } },
                        { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                        { DevicePlatform.iOS,     new[] { "public.xlsx" } },
                        { DevicePlatform.macOS,   new[] { ".xlsx" } }
                    })
                });
                if (file is null) return;

                var imported = await _excel.ImportItemsAsync(file.FullPath);

                foreach (var it in imported)
                {
                    await _db.AddItemAsync(it);
                    Items.Add(it);
                }

                await Toast.Make("Ítems importados", ToastDuration.Short).Show();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error importando Excel", ex.InnerException?.Message ?? ex.Message, "OK");
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --------------------------------------------------------------------
        //  Exportar a Excel
        // --------------------------------------------------------------------
        private async Task ExportItemsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var exportPath = Path.Combine(FileSystem.AppDataDirectory, "items_export.xlsx");
                var allItems = await _db.GetItemsAsync();

                await _excel.ExportItemsAsync(exportPath, allItems);
                await Toast.Make($"Exportado en: {exportPath}", ToastDuration.Short).Show();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error exportando Excel", ex.Message, "OK");
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --------------------------------------------------------------------
        //  Operaciones sobre un ítem individual
        // --------------------------------------------------------------------
        private async Task AddItemAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
            Debug.WriteLine("AddItem ejecutado");

                var nuevo = new Item
                {
                    Numero = string.Empty, // lo rellenará el usuario
                    Descripcion = string.Empty,
                    Unidad = string.Empty,
                    Cantidad = 0,
                    PrecioUnitario = 0m,
                    UsarEnSeguimiento = false
                };

                await _db.AddItemAsync(nuevo); // guarda en SQLite
                Items.Add(nuevo);              // refresca la UI
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error creando ítem", ex.InnerException?.Message ?? ex.Message, "OK");
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteItemAsync(Item item)
        {
            if (item is null || IsBusy) return;
            IsBusy = true;
            try
            {
                await _db.DeleteItemAsync(item.Id);
                Items.Remove(item);
                await Toast.Make("Ítem eliminado", ToastDuration.Short).Show();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error eliminando ítem", ex.InnerException?.Message ?? ex.Message, "OK");
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task EditItemAsync(Item item)
        {
            if (item is null || IsBusy) return;
            IsBusy = true;
            try
            {
                await Shell.Current.GoToAsync($"item/edit?id={item.Id}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

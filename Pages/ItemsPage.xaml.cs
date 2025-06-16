using atajados.PageModels;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace atajados.Pages
{
    public partial class ItemsPage : ContentPage
    {
        // ① Constructor sin argumentos ─ usado cuando la página la crea XAML/Shell
        public ItemsPage() : this(
            App.Current!.Services.GetRequiredService<ItemsViewModel>())
        {
        }

        // ② Constructor inyectado ─ útil si algún día creas la página vía DI
        public ItemsPage(ItemsViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}

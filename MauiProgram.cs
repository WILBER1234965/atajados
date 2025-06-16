using atajados;
using atajados.Data;
using atajados.PageModels;
using atajados.Pages;
using atajados.Services;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;
using System.Threading.Tasks;

namespace atajados
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // ---------------------------------------------------------------
            //  SQLite DB
            // ---------------------------------------------------------------
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlite($"Filename={FileSystem.AppDataDirectory}/atajados.db"));

            // ---------------------------------------------------------------
            //  Servicios
            // ---------------------------------------------------------------
            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<ExcelService>();

            // ---------------------------------------------------------------
            //  Shell, Páginas y ViewModels
            // ---------------------------------------------------------------
            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddTransient<ItemsPage>();
            builder.Services.AddTransient<ItemsViewModel>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainPageModel>();

            builder.Services.AddTransient<ProjectListPage>();
            builder.Services.AddTransient<ProjectListPageModel>();

            builder.Services.AddTransient<ProjectDetailPage>();
            builder.Services.AddTransient<ProjectDetailPageModel>();

            builder.Services.AddTransient<TaskDetailPage>();
            builder.Services.AddTransient<TaskDetailPageModel>();

            builder.Services.AddTransient<ManageMetaPage>();
            builder.Services.AddTransient<ManageMetaPageModel>();

            // ---------------------------------------------------------------
            //  Construye la app
            // ---------------------------------------------------------------
            var app = builder.Build();

            // ---------------------------------------------------------------
            //  Inicializa (borra y migra) la BD antes de mostrar la UI
            // ---------------------------------------------------------------
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                // Espera a que termine la tarea para no seguir con esquemas viejos
                Task.Run(() => db.InitializeAsync()).Wait();
            }

            return app;
        }
    }
}

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using QuickRegister.Data;
using QuickRegister.ViewModels;
using QuickRegister.Views;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace QuickRegister
{
    public partial class TijdregistratieApp : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
#if DEBUG
                var dbPath = Path.Combine(System.Environment.CurrentDirectory, "elumatec.db");
#else
                var dbPath = Path.Combine(AppContext.BaseDirectory, "elumatec.db");
#endif
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite($"Data Source={dbPath}")
                    .Options;

                var dbContext = new AppDbContext(options);
                var mainViewModel = new MainViewModel(dbContext);

                var window = new TijdregistratieWindow
                {
                    DataContext = mainViewModel
                };

                desktop.MainWindow = window;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
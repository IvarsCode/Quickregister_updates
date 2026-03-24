using Avalonia.Controls;
using QuickRegister.Data;
using QuickRegister.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace QuickRegister.Views
{
    public partial class TijdregistratieWindow : Window
    {
        public TijdregistratieWindow()
        {
            InitializeComponent();

            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "elumatec.db");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            var dbContext = new AppDbContext(options);

            DataContext = new MainViewModel(dbContext);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (DataContext is MainViewModel main &&
                main.CurrentView is IClosingGuard guard)
            {
                if (!guard.OnWindowCloseRequested())
                    e.Cancel = true;
            }

            base.OnClosing(e);
        }
    }
}

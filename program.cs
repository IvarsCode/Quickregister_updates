using Avalonia;
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuickRegister.Data;

namespace QuickRegister
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
#if DEBUG
            var dbPath = Path.Combine(Environment.CurrentDirectory, "elumatec.db");
#else
            var dbPath = Path.Combine(AppContext.BaseDirectory, "elumatec.db");
#endif
            Console.WriteLine($"=== DATABASE INITIALIZATION ===");
            Console.WriteLine($"Database path: {dbPath}");
            Console.WriteLine($"Database exists: {File.Exists(dbPath)}");

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                .Options;

            try
            {
                using (var db = new AppDbContext(options))
                {
#if DEBUG
                    db.Database.Migrate();
#else
                    db.Database.EnsureCreated();
#endif
                    BedrijvenLaden.LoadBedrijvenCsvToDb(db);
                    MachineLaden.LoadMachineCsvToDb(db);
                    MedewerkersLaden.LoadMedewerkerCsvToDb(db);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n=== ERROR OCCURRED ===");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"\n=== INNER EXCEPTION ===");
                    Console.WriteLine($"Type: {ex.InnerException.GetType().Name}");
                    Console.WriteLine($"Message: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack Trace:\n{ex.InnerException.StackTrace}");
                }

                Console.WriteLine("\n=== Press any key to exit ===");
                Console.ReadKey();
                Environment.Exit(1);
            }

            Console.WriteLine("Starting Avalonia UI...");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<TijdregistratieApp>()
                         .UsePlatformDetect()
                         .LogToTrace();
    }
}
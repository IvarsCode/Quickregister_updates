using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuickRegister.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "elumatec.db");
        builder.UseSqlite($"Data Source={dbPath}");
        return new AppDbContext(builder.Options);
    }
}

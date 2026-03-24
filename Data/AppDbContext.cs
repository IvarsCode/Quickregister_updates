using Microsoft.EntityFrameworkCore;
using QuickRegister.Models;

namespace QuickRegister.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Medewerker> Medewerkers { get; set; } = null!;
    public DbSet<Bedrijf> Bedrijven { get; set; } = null!;
    public DbSet<Interventie> Interventies { get; set; } = null!;
    public DbSet<InterventieCall> InterventieCalls { get; set; } = null!;
    public DbSet<AppState> AppState { get; set; } = null!;
    public DbSet<Machine> Machines { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medewerker>().ToTable("medewerkers");
        modelBuilder.Entity<Bedrijf>().ToTable("bedrijven");
        modelBuilder.Entity<Interventie>().ToTable("interventies");
        modelBuilder.Entity<InterventieCall>().ToTable("interventie_call");
        modelBuilder.Entity<Machine>().ToTable("Machines");

        modelBuilder.Entity<AppState>()
            .ToTable("app_state")
            .HasKey(x => x.Key);

        // Id behavior
        modelBuilder.Entity<Machine>()
            .HasKey(e => e.MachineNaam);

        modelBuilder.Entity<Bedrijf>()
            .Property(b => b.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Interventie>()
            .Property(i => i.Id)
            .ValueGeneratedNever(); // manual ID

        modelBuilder.Entity<InterventieCall>()
            .Property(c => c.Id)
            .ValueGeneratedNever(); // manual ID

        modelBuilder.Entity<Medewerker>()
            .Property(m => m.Id)
            .ValueGeneratedOnAdd();

        // Configure DateTime columns for SQLite 
        modelBuilder.Entity<InterventieCall>()
            .Property(c => c.StartCall)
            .HasColumnType("TEXT");

        modelBuilder.Entity<InterventieCall>()
            .Property(c => c.EindCall)
            .HasColumnType("TEXT");

        // InterventieCall -> Interventie
        modelBuilder.Entity<InterventieCall>()
            .HasOne(c => c.Interventie)
            .WithMany(i => i.Calls)
            .HasForeignKey(c => c.InterventieId)
            .OnDelete(DeleteBehavior.Restrict);

        // InterventieCall -> Medewerker
        modelBuilder.Entity<InterventieCall>()
            .HasOne(c => c.Medewerker)
            .WithMany()
            .HasForeignKey(c => c.MedewerkerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Interventie -> Afgerond defaults
        modelBuilder.Entity<Interventie>()
            .Property(i => i.Afgerond)
            .HasDefaultValue(0);
    }
}

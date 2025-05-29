using Microsoft.EntityFrameworkCore;
using TermProject.Models;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Repair> Repairs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>().Property(c => c.FirstName).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Client>().Property(c => c.LastName).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Client>().Property(c => c.Phone).HasMaxLength(15).IsRequired();
        modelBuilder.Entity<Client>().Property(c => c.Password).HasMaxLength(100).IsRequired();

        modelBuilder.Entity<Car>().Property(c => c.VIN).HasMaxLength(17).IsRequired();
        modelBuilder.Entity<Car>().Property(c => c.LicensePlate).HasMaxLength(10).IsRequired();
        modelBuilder.Entity<Car>().Property(c => c.Model).HasMaxLength(30).IsRequired();

        modelBuilder.Entity<Repair>().Property(r => r.Description).HasMaxLength(250);
        modelBuilder.Entity<Repair>().Property(r => r.Status).HasMaxLength(20).IsRequired();
    }

}
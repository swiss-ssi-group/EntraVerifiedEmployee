using Microsoft.EntityFrameworkCore;

namespace IssuerVerifiableEmployee.Persistence;

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Employee>().HasKey(m => m.Id);

        base.OnModelCreating(builder);
    }
}

using Microsoft.EntityFrameworkCore;
using EventAccessControl.API.Models;

namespace EventAccessControl.API.Data
{
public class ApplicationDbContext : DbContext
{
public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
: base(options)
{
}

public DbSet<Event> Events { get; set; }
public DbSet<Ticket> Tickets { get; set; }
public DbSet<CheckInLog> CheckInLogs { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
base.OnModelCreating(modelBuilder);

modelBuilder.Entity<Ticket>()
.Property(t => t.RowVersion)
.IsRowVersion();
}
}
}

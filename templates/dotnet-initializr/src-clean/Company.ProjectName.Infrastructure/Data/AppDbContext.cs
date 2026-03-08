#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
#if (IncludeAspNetIdentity)
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
#endif
using Company.ProjectName.Domain.Entities;

namespace Company.ProjectName.Infrastructure.Data;

#if (IncludeAspNetIdentity)
public class AppDbContext : IdentityDbContext<IdentityUser>
#else
public class AppDbContext : DbContext
#endif
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure entity mappings here
    }
}
#endif

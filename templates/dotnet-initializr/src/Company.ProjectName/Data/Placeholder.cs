#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
#if (IncludeAspNetIdentity)
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
#endif

namespace Company.ProjectName.Data;

#if (IncludeAspNetIdentity)
public class AppDbContext : IdentityDbContext<IdentityUser>
#else
public class AppDbContext : DbContext
#endif
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Models.SampleEntity> SampleEntities => Set<Models.SampleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure entity mappings here
    }
}
#elif (IncludeDapper)
using System.Data;
using Dapper;

namespace Company.ProjectName.Data;

public class SampleRepository
{
    private readonly IDbConnection _connection;

    public SampleRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<Models.SampleEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _connection.QueryAsync<Models.SampleEntity>("SELECT Id, Name, CreatedAt FROM SampleEntities");
    }

    public async Task<Models.SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _connection.QueryFirstOrDefaultAsync<Models.SampleEntity>(
            "SELECT Id, Name, CreatedAt FROM SampleEntities WHERE Id = @Id", new { Id = id });
    }
}
#else
namespace Company.ProjectName.Data;

// Add your data access classes here.
#endif

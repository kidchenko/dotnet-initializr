#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
using Company.ProjectName.Domain.Entities;
using Company.ProjectName.Domain.Repositories;

namespace Company.ProjectName.Infrastructure.Data;

public class SampleRepository : ISampleRepository
{
    private readonly AppDbContext _context;

    public SampleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SampleEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SampleEntities.ToListAsync(cancellationToken);
    }

    public async Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SampleEntities.FindAsync(new object[] { id }, cancellationToken);
    }
}
#elif (IncludeDapper)
using System.Data;
using Dapper;
using Company.ProjectName.Domain.Entities;
using Company.ProjectName.Domain.Repositories;

namespace Company.ProjectName.Infrastructure.Data;

public class SampleRepository : ISampleRepository
{
    private readonly IDbConnection _connection;

    public SampleRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<SampleEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _connection.QueryAsync<SampleEntity>("SELECT Id, Name, CreatedAt FROM SampleEntities");
    }

    public async Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _connection.QueryFirstOrDefaultAsync<SampleEntity>(
            "SELECT Id, Name, CreatedAt FROM SampleEntities WHERE Id = @Id", new { Id = id });
    }
}
#else
using Company.ProjectName.Application;
using Company.ProjectName.Domain.Entities;

namespace Company.ProjectName.Infrastructure.Data;

public class SampleRepository : ISampleService
{
    public Task<IEnumerable<SampleEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Replace with actual data access
        return Task.FromResult<IEnumerable<SampleEntity>>(Array.Empty<SampleEntity>());
    }

    public Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: Replace with actual data access
        return Task.FromResult<SampleEntity?>(null);
    }
}
#endif

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

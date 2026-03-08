#if (IncludeAnyOrm)
using Company.ProjectName.Domain.Entities;

namespace Company.ProjectName.Domain.Repositories;

public interface ISampleRepository
{
    Task<IEnumerable<SampleEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
#endif

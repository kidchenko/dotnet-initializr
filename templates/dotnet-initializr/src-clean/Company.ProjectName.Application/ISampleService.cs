using Company.ProjectName.Domain.Entities;

namespace Company.ProjectName.Application;

public interface ISampleService
{
    Task<IEnumerable<SampleEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SampleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

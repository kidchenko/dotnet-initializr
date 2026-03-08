#if (IncludeMapping)
using Mapster;

namespace Company.ProjectName.Application;

/// <summary>
/// Sample Mapster mapping configuration. Replace with your own mappings.
/// </summary>
public record SampleSource(Guid Id, string Name, DateTime CreatedAt);
public record SampleDto(Guid Id, string Name);

public class SampleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SampleSource, SampleDto>();
    }
}
#endif

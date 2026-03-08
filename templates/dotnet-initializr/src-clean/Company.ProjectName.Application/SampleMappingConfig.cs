#if (IncludeMapping)
using Mapster;
using Company.ProjectName.Domain.Entities;

namespace Company.ProjectName.Application;

public class SampleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SampleEntity, SampleDto>();
    }
}

public record SampleDto(Guid Id, string Name);
#endif

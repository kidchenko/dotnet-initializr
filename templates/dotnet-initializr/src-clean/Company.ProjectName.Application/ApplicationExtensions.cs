using Microsoft.Extensions.DependencyInjection;
#if (IncludeValidation)
using FluentValidation;
#endif

namespace Company.ProjectName.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
#if (IncludeValidation)
        services.AddValidatorsFromAssembly(typeof(ApplicationExtensions).Assembly);
#endif
#if (IncludeMapping)
        services.AddSingleton(Mapster.TypeAdapterConfig.GlobalSettings);
        services.AddScoped<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();
#endif
        // Register additional application services here
        return services;
    }
}

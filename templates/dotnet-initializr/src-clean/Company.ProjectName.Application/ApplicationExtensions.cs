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
        services.AddValidatorsFromAssemblyContaining<ApplicationExtensions>();
#endif
#if (IncludeMapping)
        Mapster.TypeAdapterConfig.GlobalSettings.Scan(typeof(ApplicationExtensions).Assembly);
        services.AddMapster();
#endif
        // Register additional application services here
        return services;
    }
}

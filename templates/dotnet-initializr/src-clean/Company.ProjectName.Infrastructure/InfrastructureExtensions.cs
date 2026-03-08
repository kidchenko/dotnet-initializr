using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#if (IncludeEfCore)
using Microsoft.EntityFrameworkCore;
using Company.ProjectName.Infrastructure.Data;
#endif
#if (IncludeDapper)
using System.Data;
#if (IncludePostgreSql)
using Npgsql;
#elif (IncludeSqlServer)
using Microsoft.Data.SqlClient;
#elif (IncludeMySql)
using MySqlConnector;
#elif (IncludeSqlite)
using Microsoft.Data.Sqlite;
#endif
#endif
#if (IncludeAnyOrm)
using Company.ProjectName.Domain.Repositories;
#endif

namespace Company.ProjectName.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
#if (IncludeEfCore)
        services.AddDbContext<AppDbContext>(options =>
        {
#if (IncludePostgreSql)
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection")));
#elif (IncludeSqlite)
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
#endif
        });
        // To create migrations: dotnet ef migrations add Initial --project ../Company.ProjectName.Infrastructure
        // To apply migrations: dotnet ef database update --project ../Company.ProjectName.Infrastructure

        services.AddScoped<ISampleRepository, SampleRepository>();
#endif
#if (IncludeDapper)
        services.AddScoped<IDbConnection>(sp =>
        {
#if (IncludePostgreSql)
            return new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlServer)
            return new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeMySql)
            return new MySqlConnection(configuration.GetConnectionString("DefaultConnection"));
#elif (IncludeSqlite)
            return new SqliteConnection(configuration.GetConnectionString("DefaultConnection"));
#endif
        });

        services.AddScoped<ISampleRepository, SampleRepository>();
#endif

        return services;
    }
}

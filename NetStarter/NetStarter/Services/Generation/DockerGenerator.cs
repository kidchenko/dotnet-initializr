using NetStarter.Models;

namespace NetStarter.Services.Generation;

public static class DockerGenerator
{
    public static string GenerateDockerfile(ProjectConfiguration config)
    {
        var major = GetSdkMajorVersion(config.SdkVersion);
        var isConsoleOrWorker = config.ProjectType is ProjectType.Console or ProjectType.WorkerService;
        var isCleanArch = config.Architecture == ArchitecturePattern.CleanArchitecture;

        var baseImage = isConsoleOrWorker
            ? $"mcr.microsoft.com/dotnet/runtime:{major}.0"
            : $"mcr.microsoft.com/dotnet/aspnet:{major}.0";

        var sdkImage = $"mcr.microsoft.com/dotnet/sdk:{major}.0";
        var projectName = config.ProjectName;
        var tf = NuGetVersionMap.GetTargetFramework(config.SdkVersion);

        var entrypoint = isCleanArch
            ? $"""["dotnet", "{projectName}.Api.dll"]"""
            : $"""["dotnet", "{projectName}.dll"]""";

        if (isCleanArch)
        {
            return $$"""
FROM {{baseImage}} AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM {{sdkImage}} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/{{projectName}}.Api/{{projectName}}.Api.csproj", "src/{{projectName}}.Api/"]
COPY ["src/{{projectName}}.Application/{{projectName}}.Application.csproj", "src/{{projectName}}.Application/"]
COPY ["src/{{projectName}}.Domain/{{projectName}}.Domain.csproj", "src/{{projectName}}.Domain/"]
COPY ["src/{{projectName}}.Infrastructure/{{projectName}}.Infrastructure.csproj", "src/{{projectName}}.Infrastructure/"]
RUN dotnet restore "src/{{projectName}}.Api/{{projectName}}.Api.csproj"
COPY . .
WORKDIR "/src/src/{{projectName}}.Api"
RUN dotnet build "{{projectName}}.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "{{projectName}}.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT {{entrypoint}}
""";
        }
        else
        {
            return $$"""
FROM {{baseImage}} AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM {{sdkImage}} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["{{projectName}}.csproj", "."]
RUN dotnet restore "{{projectName}}.csproj"
COPY . .
RUN dotnet build "{{projectName}}.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "{{projectName}}.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT {{entrypoint}}
""";
        }
    }

    public static string GenerateDockerCompose(ProjectConfiguration config)
    {
        var projectName = config.ProjectName.ToLowerInvariant();
        var hasDatabase = config.Orm == OrmOption.EfCore && config.Database.HasValue;
        var dbServiceName = config.Database switch
        {
            DatabaseOption.PostgreSql => "postgres",
            DatabaseOption.SqlServer => "sqlserver",
            _ => string.Empty,
        };

        var dependsOn = hasDatabase
            ? $"\n      depends_on:\n        - {dbServiceName}"
            : string.Empty;

        var dbService = string.Empty;

        if (hasDatabase)
        {
            if (config.Database == DatabaseOption.PostgreSql)
            {
                dbService = $$"""

  {{dbServiceName}}:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: {{projectName}}db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
""";
            }
            else if (config.Database == DatabaseOption.SqlServer)
            {
                dbService = $$"""

  {{dbServiceName}}:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong!Passw0rd"
      ACCEPT_EULA: "Y"
      MSSQL_PID: Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
""";
            }
        }

        var volumes = string.Empty;
        if (hasDatabase)
        {
            if (config.Database == DatabaseOption.PostgreSql)
                volumes = "\nvolumes:\n  postgres_data:";
            else if (config.Database == DatabaseOption.SqlServer)
                volumes = "\nvolumes:\n  sqlserver_data:";
        }

        return $$"""
services:
  app:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development{{dependsOn}}
{{dbService}}{{volumes}}
""";
    }

    private static int GetSdkMajorVersion(DotNetSdkVersion sdk) => sdk switch
    {
        DotNetSdkVersion.Net8 => 8,
        DotNetSdkVersion.Net9 => 9,
        DotNetSdkVersion.Net10 => 10,
        _ => 9,
    };
}

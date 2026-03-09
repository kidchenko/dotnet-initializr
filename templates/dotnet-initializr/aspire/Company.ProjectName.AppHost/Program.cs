var builder = DistributedApplication.CreateBuilder(args);

// Add infrastructure resources
#if (IncludePostgreSql)
var postgres = builder.AddPostgres("postgres").AddDatabase("mydb");
#endif
#if (IncludeSqlServer)
var sqlserver = builder.AddSqlServer("sqlserver").AddDatabase("mydb");
#endif
#if (IncludeCaching)
var redis = builder.AddRedis("redis");
#endif

// Add the API project
#if (!IncludeCleanArchitecture)
var api = builder.AddProject("api", "../src/Company.ProjectName/Company.ProjectName.csproj")
#else
var api = builder.AddProject("api", "../src/Company.ProjectName.__EntryPoint__/Company.ProjectName.__EntryPoint__.csproj")
#endif
#if (IncludePostgreSql)
    .WithReference(postgres)
#endif
#if (IncludeSqlServer)
    .WithReference(sqlserver)
#endif
#if (IncludeCaching)
    .WithReference(redis)
#endif
    ;

builder.Build().Run();

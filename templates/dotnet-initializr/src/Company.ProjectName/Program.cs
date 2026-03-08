#if (IncludeWebApi)
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

//-:cnd:noEmit
#if DEBUG
app.UseDeveloperExceptionPage();
#endif
//+:cnd:noEmit

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
#elif (IncludeMinimalApi)
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

//-:cnd:noEmit
#if DEBUG
app.UseDeveloperExceptionPage();
#endif
//+:cnd:noEmit

app.UseHttpsRedirection();

app.MapGet("/hello", () => "Hello World");

app.Run();
#elif (IncludeConsole)
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register your services here
    })
    .Build();

Console.WriteLine("Hello from Company.ProjectName!");

await host.RunAsync();
#elif (IncludeWorker)
using Company.ProjectName;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
#endif

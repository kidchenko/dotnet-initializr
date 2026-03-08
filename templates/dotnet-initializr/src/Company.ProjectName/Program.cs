var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

//-:cnd:noEmit
#if DEBUG
app.UseDeveloperExceptionPage();
#endif
//+:cnd:noEmit

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapOpenApi();

app.Run();

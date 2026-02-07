using PlatformService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PlatformService.SyncDataServices.Http;
using Microsoft.OpenApi;
using PlatformService.AsyncDataServices;
using PlatformService.SyncDataServices.Grpc;
;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
services.AddOpenApi();
if (builder.Environment.IsProduction())
{
    Console.WriteLine("--> Using SqlServer Db");
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("PlatformSqlConnection")));
}
else
{
    Console.WriteLine("--> Using InMem Db");
     services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMem"));
}

services.AddScoped<IPlatformRepo, PlatformRepo>();
services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
services.AddSingleton<IMessageBusClient, MessageBusAdapter>();
services.AddGrpc();
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
services.AddSwaggerGen( c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
}); //maybe add some options later


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGrpcService<GrpcPlatformService>();
    endpoints.MapGet("/protos/platforms.proto", async context =>
    {
        await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
    });
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");


PrepDb.PrepPopulation(app, app.Environment.IsProduction());

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

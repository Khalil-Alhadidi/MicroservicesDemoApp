using Microsoft.EntityFrameworkCore;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Endpoints;
using PlatformService.SyncDataServices.Grpc;
using PlatformService.SyncDataServices.Http;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddOpenApi();

if (builder.Environment.IsProduction())
{
    Console.WriteLine("--> Using SQL Server Database");

    builder.Services.AddDbContext<AppDbContext>(
        opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformConnection")));
}
else
{
    Console.WriteLine("--> Using InMemory Database");

    builder.Services.AddDbContext<AppDbContext>(
        opt => opt.UseInMemoryDatabase("InMem"));
}





builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();

builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();


// For async version, you might want to initialize on startup
////builder.Services.AddSingleton<IMessageBusClient, MessageBusClient>();
builder.Services.AddSingleton<IMessageBusClient>(provider =>
{
    var client = new MessageBusClient(provider.GetRequiredService<IConfiguration>());
    // Initialize async in background or during app startup
    return client;
});


builder.Services.AddGrpc();


Console.WriteLine($"--> CommandService Endpoint {builder.Configuration["CommandService"]}");

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

Console.WriteLine("--> Before Seeding Env: is Prod ?"+ app.Environment.IsProduction());

// Database seeding
PrepDb.PrepPopulation(app, isProd: app.Environment.IsProduction());

//app.UseHttpsRedirection(); will cause issues with Docker, so we will not use it in this example

app.UseAuthorization();

app.MapControllers();

app.MapPlatformEndpoints();

app.MapGrpcService<GrpcPlatformService>();


app.MapGet("/health", () =>  Results.Ok("Hey: Hello World from PlatformService "+DateTime.Now));

app.MapGet("/protos/platforms.proto", async context =>
{
    await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
});


app.Run();

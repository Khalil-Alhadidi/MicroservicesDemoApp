using CommandsService.AsyncDataServices;
using CommandsService.Data;
using CommandsService.Endpoints;
using CommandsService.EventProcessing;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("InMem"));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ICommandRepo, CommandRepo>();
builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddHostedService<MessageBusSubscriber>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapPlatformsEndpoints();
app.MapCommandsEndpoints();

app.MapGet("/health", () => Results.Ok("Hey: Hello World from CommandService " + DateTime.Now));

app.UseExceptionHandler();
app.UseStatusCodePages();


app.Run();


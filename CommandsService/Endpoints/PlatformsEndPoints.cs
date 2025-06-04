using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;

namespace CommandsService.Endpoints;

public static class PlatformsEndPoints
{
    public static void MapPlatformsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/c/platforms")
            .WithTags("Commands_Platforms");



        // get all platforms
        group.MapGet("/", (ICommandRepo _respository, IMapper mapper) =>
        {
            var platforms = _respository.GetAllPlatforms();
            Console.WriteLine($"--> Getting all platforms: {platforms.Count()} found.");
            return Results.Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        });

        group.MapPost("/", () =>
        {
            Console.WriteLine("--> Inbound GET # Command Service");
            return Results.Ok("--> Inbound GET # Command Service");
        });

    }
}

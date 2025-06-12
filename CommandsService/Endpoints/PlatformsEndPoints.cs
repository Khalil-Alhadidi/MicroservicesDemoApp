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
            Console.WriteLine($"--> Getting all platforms from CommandService... ");

            var platforms = _respository.GetAllPlatforms();
            
            return Results.Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        });

        group.MapPost("/", () =>
        {
            Console.WriteLine("--> Inbound POST # Command Service");
            return Results.Ok("--> Inbound POST # Command Service");
        });

    }
}

using AutoMapper;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;
using System.Collections.Generic;

namespace PlatformService.Endpoints;

public static class PlatformEndPoints
{
    public static void MapPlatformEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/platforms")
        .WithTags("Platforms");

        // get all platforms
        group.MapGet("/",  (IPlatformRepo platformRepo, IMapper mapper) =>
        {
            var platforms = platformRepo.GetAllPlatforms();
            Console.WriteLine($"--> Getting all platforms: {platforms.Count()} found.");
            return Results.Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        });

        // get a platform by id
        group.MapGet("/{id}",  (int id, IPlatformRepo platformRepo,IMapper mapper) =>
        {
            var platform = platformRepo.GetPlatformById(id);

            if (platform != null)
            {
                return Results.Ok(mapper.Map<PlatformReadDto>(platform));
            }
            return Results.NotFound();
        });

        // create a new platform
        group.MapPost("/", async (PlatformCreateDto platformCreateDto,
                            IPlatformRepo platformRepo,
                            IMapper mapper,
                            ICommandDataClient commandDataClient,
                            IMessageBusClient messageBusClient) =>
        {
            var platform = mapper.Map<Platform>(platformCreateDto);
            platformRepo.CreatePlatform(platform);
            platformRepo.SaveChanges();
            var platformReadDto = mapper.Map<PlatformReadDto>(platform);


            try
            {
                // Send Sync Message
                await commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send synchronously: {ex.Message}");
            }

            try
            {
                //Send Async Message
                var platformPublishedDto = mapper.Map<PlatformPublishedDto>(platformReadDto);
                platformPublishedDto.Event = "Platform_Published";
                await messageBusClient.PublishNewPlatformAsync(platformPublishedDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not send Asynchronously: {ex.Message}");
            }
            return Results.Created($"/api/platforms/{platformReadDto.Id}", platformReadDto);
        });
    }
}

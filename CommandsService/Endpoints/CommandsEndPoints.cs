using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using System;
using System.ComponentModel.Design;

namespace CommandsService.Endpoints;

public static class CommandsEndPoints
{
    public static void MapCommandsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/c/platforms/")
           .WithTags("Commands");

        group.MapGet("{platformId}/commands", (int platformId, ICommandRepo _respository, IMapper mapper) =>
        {
            Console.WriteLine($"--> Getting commands for platform {platformId}.");
            if(!_respository.PlatformExists(platformId))
            {
                return Results.NotFound();
            }
            var command = _respository.GetCommandsForPlatform(platformId);
            return Results.Ok(mapper.Map<IEnumerable<CommandReadDto>>(command));
        });

        group.MapGet("{platformId}/commands/{commandId}", (int platformId,int commandId, ICommandRepo _respository, IMapper mapper) =>
        {
            Console.WriteLine($"--> Hit GetCommands for a Platform endpoint {platformId} / {commandId}.");
            if (!_respository.PlatformExists(platformId))
            {
                return Results.NotFound();
            }
            var command = _respository.GetCommand(platformId,commandId);
            if(command ==null)
            {
                return Results.NotFound();
            }
            return Results.Ok(mapper.Map<CommandReadDto>(command));
        });


        group.MapPost("{platformId}/commands", (int platformId,CommandCreateDto commandCreateDto, ICommandRepo _respository, IMapper mapper) =>
        {
            Console.WriteLine($"--> Hit CreateCommand for a Platform endpoint {platformId}.");
            if (!_respository.PlatformExists(platformId))
            {
                return Results.NotFound();
            }
            var command = mapper.Map<Command>(commandCreateDto);
            _respository.CreateCommand(platformId, command);
            _respository.SaveChanges();
            var commandReadDto = mapper.Map<CommandReadDto>(command);
            return Results.Created($"/api/c/platforms/{platformId}/commands/{commandReadDto.Id}", commandReadDto);
        });
    }
}

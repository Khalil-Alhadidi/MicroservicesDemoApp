using AutoMapper;
using CommandsService.Models;
using Grpc.Net.Client;

namespace CommandsService.SyncDataServices.Grpc;

public class PlatformDataClient : IPlatformDataClient
{
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public PlatformDataClient(IMapper mapper,IConfiguration configuration)
    {
        _mapper = mapper;
        _configuration = configuration;
    }
    public IEnumerable<Platform> ReturnAllPlatforms()
    {
        Console.WriteLine($"--> Calling Grpc Service to fetch platforms ... {_configuration["GrpcPlatform"]!}");
        using var channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]!);
        var client = new GrpcPlatform.GrpcPlatformClient(channel);
        var request = new GetAllRequest();
        try
        {
            var reply = client.GetAllPlatforms(request);
            var platforms = _mapper.Map<IEnumerable<Platform>>(reply.Platform);
            Console.WriteLine($"--> Received {platforms.Count()} platforms from Grpc Service");
            return platforms;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not call Grpc Server : {ex.Message}");
            return Enumerable.Empty<Platform>();
        }
    }
}

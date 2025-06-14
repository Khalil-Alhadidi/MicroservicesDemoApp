using AutoMapper;
using Grpc.Core;
using PlatformService.Data;

namespace PlatformService.SyncDataServices.Grpc;

public class GrpcPlatformService : GrpcPlatform.GrpcPlatformBase
{
    private readonly IPlatformRepo _repository;
    private readonly IMapper _mapper;

    public GrpcPlatformService(IPlatformRepo repository,IMapper mapper)
    {
        this._repository = repository;
        this._mapper = mapper;
    }

    public override Task<PlatformResponse> GetAllPlatforms(GetAllRequest request,ServerCallContext context)
    {
        var response = new PlatformResponse();
        var platforms = _repository.GetAllPlatforms();

        response.Platform.AddRange(_mapper.Map<IEnumerable<GrpcPlatformModel>>(platforms));
        return Task.FromResult(response);
    }
}

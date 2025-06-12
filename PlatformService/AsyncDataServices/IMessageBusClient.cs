using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices;

public interface IMessageBusClient
{
    public Task PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto);

}

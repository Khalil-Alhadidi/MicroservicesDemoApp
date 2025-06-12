namespace CommandsService.EventProcessing;

public interface IEventProcessor
{
    Task ProcessEvent(string message);
}

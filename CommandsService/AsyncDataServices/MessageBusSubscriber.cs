using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CommandsService.AsyncDataServices;

public class MessageBusSubscriber : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IEventProcessor _eventProcessor;
    private IConnection _connection;
    private IChannel _channel;
    private string _queueName;

    public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
    {
        this._configuration = configuration;
        this._eventProcessor = eventProcessor;
        // Removed async initialization from constructor
    }

    private async Task InitializeRabbitMQAsync()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"]!,
            Port = int.Parse(_configuration["RabbitMQPort"]!)
        };

        try
        {
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
            _queueName = (await _channel.QueueDeclareAsync()).QueueName;
            await _channel.QueueBindAsync(queue: _queueName, exchange: "trigger", routingKey: "");

            Console.WriteLine("--> Listening on the Message Bus... ");

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("MessageBus Disposed");

        try
        {
            if (_channel?.IsOpen == true)
            {
                await _channel.CloseAsync();
            }

            if (_connection?.IsOpen == true)
            {
                await _connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during dispose: {ex.Message}");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
    private async Task RabbitMQ_ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ Connection Shutdown");
        await Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        await InitializeRabbitMQAsync();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            Console.WriteLine("--> Event Received!");

            var body = ea.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            Console.WriteLine($"--> Received message: {notificationMessage}");

            await _eventProcessor.ProcessEvent(notificationMessage);
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);

        // Keep the background service alive until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}

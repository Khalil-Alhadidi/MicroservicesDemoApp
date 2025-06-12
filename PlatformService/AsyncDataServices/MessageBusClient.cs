using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices
{
    

    public class MessageBusClient : IMessageBusClient, IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IChannel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private async Task InitializeAsync()
        {
            if (_connection != null && _connection.IsOpen)
                return;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;

                Console.WriteLine("--> Connected to MessageBus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
                throw;
            }
        }

        public async Task PublishNewPlatformAsync(PlatformPublishedDto platformPublishedDto)
        {
            await InitializeAsync();

            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (_connection?.IsOpen == true)
            {
                Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                await SendMessageAsync(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ connection is closed, not sending");
            }
        }

        private async Task SendMessageAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(exchange: "trigger",
                                           routingKey: "",
                                           body: body); 

            Console.WriteLine($"--> We have sent {message}");
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

       
    }
}

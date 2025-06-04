using PlatformService.Dtos;
using System.Text.Json;
using System.Text;


namespace PlatformService.SyncDataServices.Http;

public class HttpCommandDataClient : ICommandDataClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public HttpCommandDataClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendPlatformToCommand(PlatformReadDto plat)
    { 
        var httpContent = new StringContent(
            JsonSerializer.Serialize(plat),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{_configuration["CommandService"]}", httpContent);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("--> Sync POST to Command Service was OK");
        }
        else
        {
            Console.WriteLine("--> Sync POST to "+ _configuration["CommandService"]+ "   "+response.StatusCode);
            Console.WriteLine("--> Sync POST to Command Service was NOT OK");
        }
            
    }
}

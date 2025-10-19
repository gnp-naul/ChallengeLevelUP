using Api.Core.Interfaces.Services;
using Api.Domain.Entitys;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

namespace Api.Services.External
{
    public class FreeToPlayApiClientService : IFreeToPlayApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FreeToPlayApiClientService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            var baseAddress = _configuration["FreeToPlayApi:BaseAddress"];
            if (!string.IsNullOrEmpty(baseAddress))
            {
                _httpClient.BaseAddress = new Uri(baseAddress);
            }
        }

        public async Task<List<ExternalGame>> GetGamesByFilterAsync(GameRecommendation request)
        {
            try
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(request.Genre))
                {
                    queryParams.Add($"category={Uri.EscapeDataString(request.Genre)}");
                }

                if (!string.IsNullOrEmpty(request.Platform) && request.Platform.ToLower() != "all")
                {
                    queryParams.Add($"platform={Uri.EscapeDataString(request.Platform.ToLower())}");
                }

                var url = "games";
                if (queryParams.Any())
                {
                    url += "?" + string.Join("&", queryParams);
                }

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    var deserialized = JsonSerializer.Deserialize<List<ExternalGame>>(stringResponse, options);
                    return deserialized ?? new List<ExternalGame>();
                }
                else
                {
                    throw new HttpRequestException($"Código de status retornado pela API: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao chamar a API FreeToPlay: {ex.Message}");
            }
        }
    }
}
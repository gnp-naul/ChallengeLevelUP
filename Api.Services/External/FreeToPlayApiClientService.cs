//using Api.Core.Interfaces.Services;
//using Api.Domain.Entitys;
//using Microsoft.Extensions.Configuration;
//using System.Net.Http;
//using System.Text.Json;

//namespace Api.Services.External
//{
//    public class FreeToPlayApiClientService : IFreeToPlayApiClientService
//    {
//        private readonly HttpClient _httpClient;
//        private readonly IConfiguration _configuration;

//        public FreeToPlayApiClientService(HttpClient httpClient, IConfiguration configuration)
//        {
//            _httpClient = httpClient;
//            _configuration = configuration;

//            var baseAddress = _configuration["FreeToPlayApi:BaseAddress"];
//            if (!string.IsNullOrEmpty(baseAddress))
//            {
//                _httpClient.BaseAddress = new Uri(baseAddress);
//            }
//        }

//        public async Task<List<ExternalGame>> GetGamesByFilterAsync(GameRecommendation request)
//        {
//            try
//            {
//                var allGames = new List<ExternalGame>();

//                if (request.Platforms == null || !request.Platforms.Any() ||
//                    request.Platforms.Contains("all", StringComparer.OrdinalIgnoreCase))
//                {
//                    // Busca sem filtro de plataforma
//                    var games = await GetGamesFromApi(request.Genre, null);
//                    allGames.AddRange(games);
//                }
//                else
//                {
//                    // Busca para cada plataforma especificada
//                    foreach (var platform in request.Platforms)
//                    {
//                        var games = await GetGamesFromApi(request.Genre, platform);
//                        allGames.AddRange(games);
//                    }
//                }

//                // Remove duplicatas (caso algum jogo apareça em múltiplas buscas)
//                return allGames
//                    .GroupBy(g => g.Id)
//                    .Select(g => g.First())
//                    .ToList();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception($"Erro ao buscar jogos filtrados: {ex.Message}");
//            }
//        }

//        private async Task<List<ExternalGame>> GetGamesFromApi(string genre, string platform)
//        {
//            var queryParams = new List<string>();

//            if (!string.IsNullOrEmpty(genre))
//            {
//                queryParams.Add($"category={Uri.EscapeDataString(genre)}");
//            }

//            if (!string.IsNullOrEmpty(platform) && platform.ToLower() != "all")
//            {
//                // Normaliza a plataforma para o formato que a API espera
//                var normalizedPlatform = platform.ToLower() switch
//                {
//                    "pc" => "pc",
//                    "browser" => "browser",
//                    _ => platform.ToLower()
//                };
//                queryParams.Add($"platform={Uri.EscapeDataString(normalizedPlatform)}");
//            }

//            var url = "games";
//            if (queryParams.Any())
//            {
//                url += "?" + string.Join("&", queryParams);
//            }

//            var response = await _httpClient.GetAsync(url);

//            if (response.IsSuccessStatusCode)
//            {
//                var stringResponse = await response.Content.ReadAsStringAsync();

//                var options = new JsonSerializerOptions
//                {
//                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                    PropertyNameCaseInsensitive = true
//                };

//                var deserialized = JsonSerializer.Deserialize<List<ExternalGame>>(stringResponse, options);
//                return deserialized ?? new List<ExternalGame>();
//            }
//            else
//            {
//                // Log do erro, mas não quebra o fluxo para outras plataformas
//                Console.WriteLine($"Erro na API para plataforma {platform}: {response.StatusCode}");
//                return new List<ExternalGame>();
//            }
//        }
//    }
//}

using Api.Core.Interfaces.Services;
using Api.Domain.Entitys;
using Api.Infra.Caching;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Api.Services.External
{
    public class FreeToPlayApiClientService : IFreeToPlayApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IRedisCachingService _cache;

        public FreeToPlayApiClientService(
            HttpClient httpClient,
            IConfiguration configuration,
            IRedisCachingService cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _cache = cache;

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
                // Gerar chave de cache única baseada nos filtros
                var cacheKey = GenerateCacheKey(request);

                // Tentar obter do cache primeiro
                var cachedGames = await _cache.GetAsync<List<ExternalGame>>(cacheKey);
                if (cachedGames != null && cachedGames.Any())
                {
                    return cachedGames;
                }

                // Se não está em cache, buscar da API
                var allGames = new List<ExternalGame>();

                if (request.Platforms == null || !request.Platforms.Any() ||
                    request.Platforms.Contains("all", StringComparer.OrdinalIgnoreCase))
                {
                    // Busca sem filtro de plataforma
                    var games = await GetGamesFromApi(request.Genre, null);
                    allGames.AddRange(games);
                }
                else
                {
                    // Busca para cada plataforma especificada
                    foreach (var platform in request.Platforms)
                    {
                        var games = await GetGamesFromApi(request.Genre, platform);
                        allGames.AddRange(games);
                    }
                }

                // Remove duplicatas (caso algum jogo apareça em múltiplas buscas)
                var result = allGames
                    .GroupBy(g => g.Id)
                    .Select(g => g.First())
                    .ToList();

                // Armazenar no cache apenas se obteve resultados
                if (result.Any())
                {
                    await _cache.SetAsync(cacheKey, result, TimeSpan.FromHours(1));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao buscar jogos filtrados: {ex.Message}");
            }
        }

        private async Task<List<ExternalGame>> GetGamesFromApi(string genre, string platform)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(genre))
            {
                queryParams.Add($"category={Uri.EscapeDataString(genre)}");
            }

            if (!string.IsNullOrEmpty(platform) && platform.ToLower() != "all")
            {
                // Normaliza a plataforma para o formato que a API espera
                var normalizedPlatform = platform.ToLower() switch
                {
                    "pc" => "pc",
                    "browser" => "browser",
                    _ => platform.ToLower()
                };
                queryParams.Add($"platform={Uri.EscapeDataString(normalizedPlatform)}");
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
                // Log do erro, mas não quebra o fluxo para outras plataformas
                Console.WriteLine($"Erro na API para plataforma {platform}: {response.StatusCode}");
                return new List<ExternalGame>();
            }
        }

        private string GenerateCacheKey(GameRecommendation request)
        {
            var platforms = request.Platforms != null && request.Platforms.Any()
                ? string.Join("-", request.Platforms.OrderBy(p => p))
                : "all";

            return $"games_{request.Genre?.ToLower()}_{platforms}";
        }
    }
}
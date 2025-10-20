using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Xunit;
using Api.Core.Interfaces.Services;
using Api.Domain.Entitys;
using Api.Services.External;
using Microsoft.Extensions.Configuration;

namespace Api.Test.Services
{
    public class FreeToPlayApiClientServiceTest
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly FreeToPlayApiClientService _freeToPlayApiClientService;
        private readonly Mock<IConfiguration> _configurationMock;

        public FreeToPlayApiClientServiceTest()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://www.freetogame.com/api/")
            };

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(x => x["FreeToPlayApi:BaseAddress"])
                .Returns("https://www.freetogame.com/api/");

            _freeToPlayApiClientService = new FreeToPlayApiClientService(_httpClient, _configurationMock.Object);
        }

        [Fact]
        public async Task GetGamesByFilterAsync_WithValidRequest_ReturnsGames()
        {
            var expectedGames = new List<ExternalGame>
            {
                new ExternalGame { Id = 1, Title = "Test Game 1", Genre = "Shooter", Platform = "PC (Windows)" },
                new ExternalGame { Id = 2, Title = "Test Game 2", Genre = "Shooter", Platform = "Web Browser" }
            };

            var request = new GameRecommendation
            {
                Genre = "Shooter",
                Platforms = new List<string> { "pc" }
            };

            SetupHttpMessageHandler(HttpStatusCode.OK, expectedGames);

            var result = await _freeToPlayApiClientService.GetGamesByFilterAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.Title == "Test Game 1");
            Assert.Contains(result, g => g.Genre == "Shooter");
        }

        [Fact]
        public async Task GetGamesByFilterAsync_WithMultiplePlatforms_ReturnsGamesFromAllPlatforms()
        {
            var pcGames = new List<ExternalGame>
            {
                new ExternalGame { Id = 1, Title = "PC Game", Genre = "Shooter", Platform = "PC (Windows)" }
            };

            var browserGames = new List<ExternalGame>
            {
                new ExternalGame { Id = 2, Title = "Browser Game", Genre = "Shooter", Platform = "Web Browser" }
            };

            var request = new GameRecommendation
            {
                Genre = "Shooter",
                Platforms = new List<string> { "pc", "browser" }
            };

            // Simula múltiplas chamadas para diferentes plataformas
            _httpMessageHandlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(pcGames))
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(browserGames))
                });

            var result = await _freeToPlayApiClientService.GetGamesByFilterAsync(request);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.Platform.Contains("PC"));
            Assert.Contains(result, g => g.Platform.Contains("Browser"));
        }

        [Fact]
        public async Task GetGamesByFilterAsync_WhenApiFails_ThrowsException()
        {
            var request = new GameRecommendation
            {
                Genre = "Shooter",
                Platforms = new List<string> { "pc" }
            };

            SetupHttpMessageHandler(HttpStatusCode.InternalServerError, null);

            await Assert.ThrowsAsync<Exception>(() =>
                _freeToPlayApiClientService.GetGamesByFilterAsync(request));
        }

        [Fact]
        public async Task GetGamesByFilterAsync_WithNoPlatforms_ReturnsAllGames()
        {
            var expectedGames = new List<ExternalGame>
            {
                new ExternalGame { Id = 1, Title = "Game 1", Genre = "Shooter", Platform = "PC (Windows)" },
                new ExternalGame { Id = 2, Title = "Game 2", Genre = "Shooter", Platform = "Web Browser" }
            };

            var request = new GameRecommendation
            {
                Genre = "Shooter",
                Platforms = null // Sem plataforma específica
            };

            SetupHttpMessageHandler(HttpStatusCode.OK, expectedGames);

            // Act
            var result = await _freeToPlayApiClientService.GetGamesByFilterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        private void SetupHttpMessageHandler(HttpStatusCode statusCode, object content)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content != null
                    ? new StringContent(JsonSerializer.Serialize(content))
                    : new StringContent("")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }
    }
}
using Moq;
using Xunit;
using Api.Application.Services;
using Api.Application.Interfaces;
using Api.Application.Dtos;
using Api.Core.Interfaces.Services;
using Api.Core.Interfaces.Repositorys;
using Api.Domain.Entitys;

namespace Api.Test.Services
{
    public class RecommendationServiceTest
    {
        private readonly Mock<IFreeToPlayApiClientService> _freeToPlayApiClientServiceMock;
        private readonly Mock<IGameRecommendationRepository> _gameRecommendationRepositoryMock;
        private readonly Mock<IRamEstimationService> _ramEstimationServiceMock;
        private readonly RecommendationService _recommendationService;

        public RecommendationServiceTest()
        {
            _freeToPlayApiClientServiceMock = new Mock<IFreeToPlayApiClientService>();
            _gameRecommendationRepositoryMock = new Mock<IGameRecommendationRepository>();
            _ramEstimationServiceMock = new Mock<IRamEstimationService>();
            _recommendationService = new RecommendationService(
                _gameRecommendationRepositoryMock.Object,
                _freeToPlayApiClientServiceMock.Object,
                _ramEstimationServiceMock.Object);
        }

        [Fact]
        public async Task GetGameRecommendationAsync_WithValidRequest_ReturnsRecommendation()
        {
            var request = new GameRecommendationRequestDto
            {
                Genre = "shooter",
                Platforms = new List<string> { "pc" },
                AvailableRAM = 8
            };

            var externalGames = new List<ExternalGame>
            {
                new ExternalGame {
                    Id = 1,
                    Title = "Test Game",
                    Genre = "shooter",
                    Platform = "PC (Windows)",
                    Game_Url = "https://test.com/game",
                    ShortDescription = "Test description"
                }
            };

            _freeToPlayApiClientServiceMock
                .Setup(x => x.GetGamesByFilterAsync(It.IsAny<GameRecommendation>()))
                .ReturnsAsync(externalGames);

            _ramEstimationServiceMock
                .Setup(x => x.EstimateRequiredRAM(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(6); // Retorna 6GB estimados

            _ramEstimationServiceMock
                .Setup(x => x.CanRunGame(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<double>()))
                .Returns(true); // Sempre retorna compatível

            _gameRecommendationRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<GameRecommendation>()))
                .ReturnsAsync((GameRecommendation r) => r);

            var result = await _recommendationService.GetGameRecommendationAsync(request);

            Assert.NotNull(result);
            Assert.Equal("Test Game", result.Title);
            Assert.Equal("https://test.com/game", result.Link);
            Assert.Equal("shooter", result.Genre);
            Assert.Equal("PC (Windows)", result.Platform);
            Assert.True(result.IsCompatibleWithAvailableRAM);
        }

        [Fact]
        public async Task GetGameRecommendationAsync_WithNoGamesFound_ThrowsException()
        {
            var request = new GameRecommendationRequestDto
            {
                Genre = "shooter",
                Platforms = new List<string> { "pc" }
            };

            _freeToPlayApiClientServiceMock
                .Setup(x => x.GetGamesByFilterAsync(It.IsAny<GameRecommendation>()))
                .ReturnsAsync(new List<ExternalGame>());

            await Assert.ThrowsAsync<Exception>(() =>
                _recommendationService.GetGameRecommendationAsync(request));
        }

        [Fact]
        public async Task GetGameRecommendationAsync_WithIncompatibleRAM_ThrowsException()
        {
            var request = new GameRecommendationRequestDto
            {
                Genre = "shooter",
                Platforms = new List<string> { "pc" },
                AvailableRAM = 4 // RAM insuficiente
            };

            var externalGames = new List<ExternalGame>
            {
                new ExternalGame {
                    Id = 1,
                    Title = "Test Game",
                    Genre = "shooter",
                    Platform = "PC (Windows)",
                    Game_Url = "https://test.com/game"
                }
            };

            _freeToPlayApiClientServiceMock
                .Setup(x => x.GetGamesByFilterAsync(It.IsAny<GameRecommendation>()))
                .ReturnsAsync(externalGames);

            _ramEstimationServiceMock
                .Setup(x => x.EstimateRequiredRAM(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(8); // Jogo precisa de 8GB

            _ramEstimationServiceMock
                .Setup(x => x.CanRunGame(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<double>()))
                .Returns(false); // Sempre retorna incompatível

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _recommendationService.GetGameRecommendationAsync(request));

            Assert.Contains("Nenhum jogo compatível", exception.Message);
        }

        [Fact]
        public async Task GetGameRecommendationAsync_WithoutRAMFilter_ReturnsRecommendation()
        {
            var request = new GameRecommendationRequestDto
            {
                Genre = "shooter",
                Platforms = new List<string> { "pc" }
                // AvailableRAM não especificado
            };

            var externalGames = new List<ExternalGame>
            {
                new ExternalGame {
                    Id = 1,
                    Title = "Test Game",
                    Genre = "shooter",
                    Platform = "PC (Windows)",
                    Game_Url = "https://test.com/game",
                    ShortDescription = "Test description"
                }
            };

            _freeToPlayApiClientServiceMock
                .Setup(x => x.GetGamesByFilterAsync(It.IsAny<GameRecommendation>()))
                .ReturnsAsync(externalGames);

            // Não é necessário mockar o RamEstimationService quando AvailableRAM é null

            _gameRecommendationRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<GameRecommendation>()))
                .ReturnsAsync((GameRecommendation r) => r);

            var result = await _recommendationService.GetGameRecommendationAsync(request);

            Assert.NotNull(result);
            Assert.Equal("Test Game", result.Title);
            Assert.Equal("shooter", result.Genre);
            Assert.Equal("PC (Windows)", result.Platform);
        }

        [Fact]
        public async Task GetRecommendationHistoryAsync_ReturnsHistory()
        {
            var history = new List<GameRecommendation>
            {
                new GameRecommendation
                {
                    Id = 1,
                    Title = "Game 1",
                    Genre = "Shooter",
                    Platform = "PC (Windows)",
                    RecommendedAt = DateTime.UtcNow
                }
            };

            _gameRecommendationRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(history);

            var result = await _recommendationService.GetRecommendationHistoryAsync();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Game 1", result.First().Title);
        }
    }
}
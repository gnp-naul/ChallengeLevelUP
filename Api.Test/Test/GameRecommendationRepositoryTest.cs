using Api.Domain.Entitys;
using Api.Infra.Data;
using Api.Infra.Data.Repositorys;
using Microsoft.EntityFrameworkCore;

namespace Api.Test.Repositories
{
    public class GameRecommendationRepositoryTest
    {
        private readonly ApplicationDbContext _context;
        private readonly GameRecommendationRepository _repository;

        public GameRecommendationRepositoryTest()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new GameRecommendationRepository(_context);

            // Clean the database before each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task AddAsync_WithValidGameRecommendation_ReturnsSavedEntity()
        {
            var recommendation = new GameRecommendation
            {
                Title = "Test Game",
                Genre = "Shooter",
                Platform = "PC (Windows)",
                RecommendedAt = DateTime.UtcNow
            };

            var result = await _repository.AddAsync(recommendation);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("Test Game", result.Title);
            Assert.Equal("Shooter", result.Genre);
            Assert.Equal("PC (Windows)", result.Platform);

            // Verifique se ele foi realmente salvo
            var savedEntity = await _context.GameRecommendations.FindAsync(result.Id);
            Assert.NotNull(savedEntity);
            Assert.Equal(result.Id, savedEntity.Id);
        }

        [Fact]
        public async Task AddAsync_WithMultipleRecommendations_IncrementsId()
        {
            var recommendation1 = new GameRecommendation
            {
                Title = "Game 1",
                Genre = "Shooter",
                Platform = "PC (Windows)",
                RecommendedAt = DateTime.UtcNow
            };

            var recommendation2 = new GameRecommendation
            {
                Title = "Game 2",
                Genre = "Strategy",
                Platform = "Web Browser",
                RecommendedAt = DateTime.UtcNow
            };

            var result1 = await _repository.AddAsync(recommendation1);
            var result2 = await _repository.AddAsync(recommendation2);

            Assert.True(result1.Id > 0);
            Assert.True(result2.Id > 0);
            Assert.NotEqual(result1.Id, result2.Id);
        }

        [Fact]
        public async Task GetAllAsync_WithNoData_ReturnsEmptyList()
        {
            var result = await _repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WithData_ReturnsAllRecommendationsOrderedByDate()
        {
            var recommendations = new[]
            {
                new GameRecommendation
                {
                    Title = "Game 1",
                    Genre = "Shooter",
                    Platform = "PC (Windows)",
                    RecommendedAt = DateTime.UtcNow.AddHours(-2)
                },
                new GameRecommendation
                {
                    Title = "Game 2",
                    Genre = "Strategy",
                    Platform = "Web Browser",
                    RecommendedAt = DateTime.UtcNow.AddHours(-1)
                },
                new GameRecommendation
                {
                    Title = "Game 3",
                    Genre = "RPG",
                    Platform = "PC (Windows)",
                    RecommendedAt = DateTime.UtcNow
                }
            };

            foreach (var rec in recommendations)
            {
                await _repository.AddAsync(rec);
            }

            var result = await _repository.GetAllAsync();
            var resultList = result.ToList();

            Assert.NotNull(result);
            Assert.Equal(3, resultList.Count);

            // Deve ser ordenado por RecommendedAt em ordem decrescente(mais recente primeiro)
            Assert.Equal("Game 3", resultList[0].Title); //Mais recente
            Assert.Equal("Game 2", resultList[1].Title);
            Assert.Equal("Game 1", resultList[2].Title); // Mais antigo
        }

        [Fact]
        public async Task GetAllAsync_AfterAddingRecommendation_ReturnsCorrectCount()
        {
            var initialCount = (await _repository.GetAllAsync()).Count();

            var recommendation = new GameRecommendation
            {
                Title = "Test Game",
                Genre = "Shooter",
                Platform = "PC (Windows)",
                RecommendedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(recommendation);

            var result = await _repository.GetAllAsync();
            var finalCount = result.Count();

            Assert.Equal(0, initialCount);
            Assert.Equal(1, finalCount);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
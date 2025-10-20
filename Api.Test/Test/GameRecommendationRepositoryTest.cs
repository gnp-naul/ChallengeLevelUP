using Api.Domain.Entitys;
using Api.Infra.Data;
using Api.Infra.Data.Repositorys;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.Test.Repositories
{
    public class GameRecommendationRepositoryTest : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly GameRecommendationRepository _repository;

        public GameRecommendationRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Nome único para cada teste
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new GameRecommendationRepository(_context);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public async Task GetAllAsync_WithNoData_ReturnsEmptyList()
        {
            var result = await _repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_WithNullProperties_ThrowsException()
        {
            var recommendation = new GameRecommendation
            {
                Title = null, // Title nulo
                Genre = "Shooter",
                Platform = "PC (Windows)",
                RecommendedAt = DateTime.UtcNow
            };

            var exception = await Record.ExceptionAsync(() => _repository.AddAsync(recommendation));
            Assert.NotNull(exception); // Deve lançar exceção devido a constraints do EF
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
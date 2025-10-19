using Api.Core.Interfaces.Repositorys;
using Api.Domain.Entitys;
using Microsoft.EntityFrameworkCore;

namespace Api.Infra.Data.Repositorys
{
    public class GameRecommendationRepository : IGameRecommendationRepository
    {
        private readonly ApplicationDbContext _context;

        public GameRecommendationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GameRecommendation> AddAsync()
        {
            var recommendation = new GameRecommendation();
            _context.GameRecommendations.Add(recommendation);
            await _context.SaveChangesAsync();
            return recommendation;
        }

        public async Task<IEnumerable<GameRecommendation>> GetAllAsync()
        {
            return await _context.GameRecommendations
                .OrderByDescending(g => g.RecommendedAt)
                .ToListAsync();
        }
    }
}

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

        public async Task<GameRecommendation> AddAsync(GameRecommendation game)
        {
            _context.GameRecommendations.Add(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<IEnumerable<GameRecommendation>> GetAllAsync()
        {
            return await _context.GameRecommendations
                .AsNoTracking()
                .OrderByDescending(g => g.RecommendedAt)
                .ToListAsync();
        }
    }
}

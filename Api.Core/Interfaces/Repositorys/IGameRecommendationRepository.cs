using Api.Domain.Entitys;

namespace Api.Core.Interfaces.Repositorys
{
    public interface IGameRecommendationRepository
    {
        Task<GameRecommendation> AddAsync(GameRecommendation game);
        Task<IEnumerable<GameRecommendation>> GetAllAsync();
    }
}

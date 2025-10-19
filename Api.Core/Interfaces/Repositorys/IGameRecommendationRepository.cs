using Api.Domain.Entitys;

namespace Api.Core.Interfaces.Repositorys
{
    public interface IGameRecommendationRepository
    {
        Task<GameRecommendation> AddAsync();
        Task<IEnumerable<GameRecommendation>> GetAllAsync();
    }
}

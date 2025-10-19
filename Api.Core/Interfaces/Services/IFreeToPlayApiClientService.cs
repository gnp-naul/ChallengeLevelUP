using Api.Domain.Entitys;

namespace Api.Core.Interfaces.Services
{
    public interface IFreeToPlayApiClientService
    {
        Task<List<ExternalGame>> GetGamesByFilterAsync(GameRecommendation request);
    }
}

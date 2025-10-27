using Api.Application.Dtos;
using Api.Domain.Entitys;

namespace Api.Application.Mappers
{
    public static class GameMapper
    {
        // Request DTO -> GameRecommendation (para filtros da API)
        public static GameRecommendation ToGameRecommendation(this GameRecommendationRequestDto request)
        {
            return new GameRecommendation
            {
                Genre = request.Genre,
                Platforms = request.Platforms,
                AvailableRAM = request.AvailableRAM
            };
        }

        // ExternalGame -> GameRecommendation (para salvar no banco)
        public static GameRecommendation ToEntity(this ExternalGame externalGame)
        {
            return new GameRecommendation
            {
                Title = externalGame.Title,
                Link = externalGame.Game_Url,
                Genre = externalGame.Genre,
                Platform = externalGame.Platform,
                RecommendedAt = DateTime.UtcNow
            };
        }

        // ExternalGame -> Response DTO
        public static GameRecommendationResponseDto ToResponseDto(this ExternalGame externalGame)
        {
            return new GameRecommendationResponseDto
            {
                Title = externalGame.Title,
                Link = externalGame.Game_Url,
                Genre = externalGame.Genre,
                Platform = externalGame.Platform
            };
        }

        // GameRecommendation (entidade do banco) -> RecommendedGameDto (para histórico)
        public static RecommendedGameDto ToDto(this GameRecommendation recommendation)
        {
            return new RecommendedGameDto
            {
                Id = recommendation.Id,
                Title = recommendation.Title,
                Genre = recommendation.Genre,
                Platform = recommendation.Platform,
                RecommendedAt = recommendation.RecommendedAt
            };
        }
    }
}
//using Api.Application.Dtos;
//using Api.Domain.Entitys;
//using AutoMapper;

//namespace Api.Application.Mappers
//{
//    public class GameProfile : Profile
//    {
//        public GameProfile()
//        {
//            // Request DTO -> GameRecommendation
//            CreateMap<GameRecommendationRequestDto, GameRecommendation>()
//                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
//                .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => src.Platform ?? "all"))
//                .ForMember(dest => dest.AvailableRAM, opt => opt.MapFrom(src => src.AvailableRAM))
//                .ForMember(dest => dest.RecommendedAt, opt => opt.Ignore())
//                .ForMember(dest => dest.Id, opt => opt.Ignore())
//                .ForMember(dest => dest.Title, opt => opt.Ignore());

//            // ExternalGame -> GameRecommendation (para salvar no banco)
//            CreateMap<ExternalGame, GameRecommendation>()
//                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
//                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
//                .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => src.Platform))
//                .ForMember(dest => dest.RecommendedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
//                .ForMember(dest => dest.Id, opt => opt.Ignore())
//                .ForMember(dest => dest.AvailableRAM, opt => opt.Ignore());

//            // ExternalGame -> Response DTO
//            CreateMap<ExternalGame, GameRecommendationResponseDto>()
//                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
//                .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.GameUrl))
//                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
//                .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => src.Platform));
//        }
//    }
//}

// Api.Application/Mappers/GameMapper.cs
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
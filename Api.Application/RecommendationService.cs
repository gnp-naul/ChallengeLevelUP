using Api.Application.Dtos;
using Api.Application.Interfaces;
using Api.Application.Mappers;
using Api.Core.Interfaces.Repositorys;
using Api.Core.Interfaces.Services;
using Api.Domain.Entitys;
using AutoMapper;

namespace Api.Application
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IFreeToPlayApiClientService _freeToPlayApiClientService;
        private readonly IGameRecommendationRepository _gameRecommendationRepository;

        public RecommendationService(
            IGameRecommendationRepository gameRecommendationRepository,
            IFreeToPlayApiClientService freeToPlayApiClientService)
        {
            _gameRecommendationRepository = gameRecommendationRepository;
            _freeToPlayApiClientService = freeToPlayApiClientService;
        }

        public async Task<GameRecommendationResponseDto> GetGameRecommendationAsync(GameRecommendationRequestDto request)
        {
            try
            {
                var gameRecommendation = request.ToGameRecommendation();

                //bater na api externa para obter a recomendação passando os filtros do request
                var externalGames = await _freeToPlayApiClientService.GetGamesByFilterAsync(gameRecommendation);

                if (!externalGames.Any())
                {
                    throw new Exception("Nenhum jogo encontrado para os filtros especificados.");
                }

                // Selecionar aleatoriamente
                var random = new Random();
                var selectedGame = externalGames[random.Next(externalGames.Count)];

                // Salvar no banco
                var recommendationToSave = selectedGame.ToEntity();
                //await _gameRecommendationRepository.AddAsync(recommendationToSave);

                return selectedGame.ToResponseDto();
            } 
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter recomendação de jogo: " + ex.Message);
            }
        }

        //public async Task<IEnumerable<RecommendedGameDto>> GetRecommendationHistoryAsync()
        //{
        //    var recommendations = await _gameRecommendationRepository.GetAllAsync();

        //    return recommendations.Select(r => new RecommendedGameDto
        //    {
        //        Id = r.Id,
        //        GameTitle = r.GameTitle,
        //        GameGenre = r.GameGenre,
        //        Platform = r.Platform,
        //        RecommendedAt = r.RecommendedAt
        //    });
        //}
    }
}

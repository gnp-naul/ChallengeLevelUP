using Api.Application.Interfaces;
using Api.Application.Dtos;
using Api.Core.Interfaces.Services;
using Api.Core.Interfaces.Repositorys;
using Api.Domain.Entitys;
using Api.Application.Mappers;

namespace Api.Application.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IGameRecommendationRepository _gameRecommendationRepository;
        private readonly IFreeToPlayApiClientService _freeToPlayApiClientService;
        private readonly IRamEstimationService _ramEstimationService;
        private readonly Random _random;

        public RecommendationService(
            IGameRecommendationRepository gameRecommendationRepository,
            IFreeToPlayApiClientService freeToPlayApiClientService,
            IRamEstimationService ramEstimationService)
        {
            _gameRecommendationRepository = gameRecommendationRepository;
            _freeToPlayApiClientService = freeToPlayApiClientService;
            _ramEstimationService = ramEstimationService;
            _random = new Random();
        }

        public async Task<GameRecommendationResponseDto> GetGameRecommendationAsync(GameRecommendationRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Genre))
                throw new ArgumentException("Gênero é obrigatório");

            var gameRecommendation = request.ToGameRecommendation();

            var externalGames = await _freeToPlayApiClientService.GetGamesByFilterAsync(gameRecommendation);

            if (!externalGames.Any())
                throw new Exception("Nenhum jogo encontrado com os filtros especificados");

            // Selecionar um jogo aleatório que seja compatível com a RAM (se fornecida)
            ExternalGame selectedGame = await SelectCompatibleGameAsync(externalGames, request.AvailableRAM);

            // Calcular RAM estimada para o jogo selecionado
            var estimatedRam = _ramEstimationService.EstimateRequiredRAM(
                selectedGame.Genre,
                selectedGame.Platform,
                selectedGame.Title);

            var recommendationToSave = selectedGame.ToEntity();
            await _gameRecommendationRepository.AddAsync(recommendationToSave);

            // Retornar resposta com informações de RAM
            var response = selectedGame.ToResponseDto();
            response.EstimatedRequiredRAM = estimatedRam;
            response.IsCompatibleWithAvailableRAM = true;

            return response;
        }

        public async Task<IEnumerable<RecommendedGameDto>> GetRecommendationHistoryAsync()
        {
            try
            {
                var recommendations = await _gameRecommendationRepository.GetAllAsync();

                if (!recommendations.Any())
                {
                    throw new Exception("Nenhum jogo encontrado.");
                }

                return recommendations.Select(GameMapper.ToDto);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter histórico de recomendações: " + ex.Message);
            }
        }

        private async Task<ExternalGame> SelectCompatibleGameAsync(List<ExternalGame> games, int? availableRAM)
        {
            if (!availableRAM.HasValue)
            {
                return games[_random.Next(games.Count)];
            }

            // Estratégia: tenta encontrar um jogo compatível em até 10 tentativas
            const int maxAttempts = 10;
            var attempts = 0;
            var shuffledGames = Shuffle(games); // Embaralha para não pegar sempre os mesmos jogos

            foreach (var game in shuffledGames.Take(maxAttempts))
            {
                var requiredRAM = _ramEstimationService.EstimateRequiredRAM(
                    game.Genre, game.Platform, game.Title);

                if (_ramEstimationService.CanRunGame(availableRAM.Value, requiredRAM))
                {
                    return game;
                }
                attempts++;
            }

            // Se não encontrou em 10 tentativas, busca exaustivamente
            var compatibleGame = shuffledGames.FirstOrDefault(game =>
            {
                var requiredRAM = _ramEstimationService.EstimateRequiredRAM(
                    game.Genre, game.Platform, game.Title);
                return _ramEstimationService.CanRunGame(availableRAM.Value, requiredRAM);
            });

            if (compatibleGame != null)
            {
                return compatibleGame;
            }

            throw new Exception(
                $"Nenhum jogo compatível com {availableRAM}GB de RAM encontrado. " +
                "Tente aumentar a memória disponível ou ajustar os filtros.");
        }

        private List<ExternalGame> Shuffle(List<ExternalGame> list)
        {
            var shuffled = list.ToList();
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }
            return shuffled;
        }
    }
}
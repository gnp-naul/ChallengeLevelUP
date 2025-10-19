using Api.Application.Dtos;
using Api.Application.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace ChallengeLevelUP.Controllers
{
    [Route("api/")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpPost("recommend")]
        public async Task<ActionResult<GameRecommendationResponseDto>> GetGameRecommendation(
            [FromBody] GameRecommendationRequestDto request)
        {
            return Ok(await _recommendationService.GetGameRecommendationAsync(request));
        }

        //[HttpGet("history")]
        //public async Task<ActionResult<IEnumerable<RecommendedGameDto>>> GetRecommendationHistory()
        //{
        //    var history = await _recommendationService.GetRecommendationHistoryAsync();
        //    return Ok(history);
        //}
    }
}

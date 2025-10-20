using Xunit;
using Api.Services.External;
using Api.Core.Interfaces.Services;

namespace Api.Test.Services
{
    public class RamEstimationServiceTest
    {
        private readonly IRamEstimationService _ramEstimationService;

        public RamEstimationServiceTest()
        {
            _ramEstimationService = new RamEstimationService();
        }

        [Fact]
        public void CanRunGame_WithSafetyMargin_RespectsMargin()
        {
            var result = _ramEstimationService.CanRunGame(10, 8, 0.2);

            Assert.True(result);
        }
    }
}
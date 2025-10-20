using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Core.Interfaces.Services
{
    public interface IRamEstimationService
    {
        int EstimateRequiredRAM(string genre, string platform, string title = null);
        bool CanRunGame(int availableRAM, int requiredRAM, double safetyMargin = 0.2);
    }
}

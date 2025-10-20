using Api.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services.External
{
    public class RamEstimationService : IRamEstimationService
    {
        public int EstimateRequiredRAM(string genre, string platform, string title = null)
        {
            var genreKey = genre?.ToLower() ?? "default";
            var platformKey = platform?.ToLower() ?? "pc";

            // RAM base baseada na plataforma
            var baseRam = _platformRamRequirements.GetValueOrDefault(platformKey, 4);

            // Adiciona RAM baseada no gênero
            var genreRam = _genreRamRequirements.GetValueOrDefault(genreKey, 4);

            // Calcula RAM total (média ponderada)
            var estimatedRam = (baseRam + genreRam) / 2;

            // Ajustes específicos baseados no título (para jogos conhecidos)
            if (!string.IsNullOrEmpty(title))
            {
                estimatedRam = ApplyTitleBasedAdjustments(title, estimatedRam);
            }

            return Math.Max(2, estimatedRam); // Mínimo de 2GB
        }

        public bool CanRunGame(int availableRAM, int requiredRAM, double safetyMargin = 0.2)
        {
            // Adiciona uma margem de segurança (20% por padrão)
            var requiredWithMargin = requiredRAM * (1 + safetyMargin);
            return availableRAM >= requiredWithMargin;
        }

        private int ApplyTitleBasedAdjustments(string title, int currentEstimate)
        {
            var lowerTitle = title.ToLower();

            // Jogos conhecidos por serem pesados
            if (lowerTitle.Contains("crysis") || lowerTitle.Contains("cyberpunk") ||
                lowerTitle.Contains("star citizen") || lowerTitle.Contains("flight simulator"))
            {
                return Math.Max(currentEstimate, 16);
            }

            // Jogos conhecidos por serem leves
            if (lowerTitle.Contains("among us") || lowerTitle.Contains("fall guys") ||
                lowerTitle.Contains("minecraft") || lowerTitle.Contains("stardew valley"))
            {
                return Math.Min(currentEstimate, 4);
            }

            return currentEstimate;
        }

        private readonly Dictionary<string, int> _genreRamRequirements = new()
        {
            ["shooter"] = 8,
            ["action"] = 6,
            ["battle-royale"] = 12,
            ["mmo"] = 8,
            ["mmorpg"] = 8,
            ["strategy"] = 4,
            ["rpg"] = 6,
            ["racing"] = 6,
            ["sports"] = 4,
            ["fantasy"] = 6,
            ["card"] = 2,
            ["fighting"] = 4,
            ["moba"] = 4,
            ["social"] = 2
        };

        private readonly Dictionary<string, int> _platformRamRequirements = new()
        {
            ["pc"] = 4,  // Base para PC
            ["browser"] = 2  // Base para navegador
        };
    }
}

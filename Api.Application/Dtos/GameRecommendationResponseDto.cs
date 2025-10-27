using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Dtos
{
    public class GameRecommendationResponseDto
    {
        public string? Title { get; set; }
        public string? Link { get; set; }
        public string? Genre { get; set; }
        public string? Platform { get; set; }
        public int EstimatedRequiredRAM { get; set; } // Em GB
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Dtos
{
    public class RecommendedGameDto
    {
        public int Id { get; set; }
        public string? GameTitle { get; set; }
        public string? GameGenre { get; set; }
        public string? Platform { get; set; }
        public DateTime RecommendedAt { get; set; }
        public RecommendedGameDto()
        {
            RecommendedAt = DateTime.UtcNow;
        }
    }
}

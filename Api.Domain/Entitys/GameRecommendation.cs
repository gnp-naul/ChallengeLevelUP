using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Entitys
{
    public class GameRecommendation
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public string Link { get; set; }

        [NotMapped]
        public List<string> Platforms { get; set; }
        public string Platform { get; set; }
        public int? AvailableRAM { get; set; }
        public DateTime RecommendedAt { get; set; }

        public GameRecommendation()
        {
            RecommendedAt = DateTime.UtcNow;
        }
    }
}

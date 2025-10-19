using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Entitys
{
    public class ExternalGame
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Thumbnail { get; set; }
        public string? ShortDescription { get; set; }
        public string? Game_Url { get; set; }
        public string? Genre { get; set; }
        public string? Platform { get; set; }
        public string? Publisher { get; set; }
        public string? Developer { get; set; }
        public string? ReleaseDate { get; set; }
        public string? FreetogameProfileUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Application.Dtos
{
    public class GameRecommendationRequestDto
    {
        [Required(ErrorMessage = "O 'Gênero' é obrigatório.")]
        [GenreValidation]
        public required string Genre { get; set; }

        public string? Platform { get; set; }
        public int? AvailableRAM { get; set; }
    }

    public class GenreValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string genre && !string.IsNullOrWhiteSpace(genre))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage ?? "O 'Gênero' é obrigatório e não pode estar vazio.");
        }
    }
}

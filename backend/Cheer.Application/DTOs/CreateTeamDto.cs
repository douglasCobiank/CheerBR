using System.ComponentModel.DataAnnotations;

namespace Cheer.Application.DTOs
{
    public class CreateTeamDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(150, ErrorMessage = "Nome não pode exceder 150 caracteres")]
        public required string Nome { get; set; }

        [StringLength(150)]
        public string? Programa { get; set; }

        [Range(1, 6, ErrorMessage = "Nível deve estar entre 1 e 6")]
        public int? Nivel { get; set; }

        [Required(ErrorMessage = "Cidade é obrigatória")]
        [StringLength(100)]
        public required string Cidade { get; set; }

        [StringLength(2, MinimumLength = 2, ErrorMessage = "Estado deve ter 2 caracteres")]
        public string Estado { get; set; } = "PR";

        [Required(ErrorMessage = "Categoria é obrigatória")]
        [StringLength(50)]
        public required string Categoria { get; set; }

        [StringLength(100)]
        public string? Instagram { get; set; }

        [StringLength(100)]
        public string? Facebook { get; set; }

        [StringLength(150)]
        public string? Coach { get; set; }

        [StringLength(20)]
        public string? Fundacao { get; set; }

        [Required(ErrorMessage = "Status é obrigatório")]
        [StringLength(30)]
        public required string Status { get; set; }

        [Url(ErrorMessage = "LogoUrl deve ser uma URL válida")]
        public string? LogoUrl { get; set; }
    }
}

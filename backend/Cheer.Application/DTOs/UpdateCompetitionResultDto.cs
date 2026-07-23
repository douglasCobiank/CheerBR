using System.ComponentModel.DataAnnotations;

namespace Cheer.Application.DTOs
{
    public class UpdateCompetitionResultDto
    {
        [Range(1900, 2100, ErrorMessage = "Ano inválido")]
        public int Ano { get; set; }

        [Required(ErrorMessage = "NomeCampeonato é obrigatório")]
        [StringLength(200)]
        public required string NomeCampeonato { get; set; }

        [Required(ErrorMessage = "Importancia é obrigatória")]
        [StringLength(30)]
        public required string Importancia { get; set; }

        [Range(1, 6, ErrorMessage = "Nível deve estar entre 1 e 6")]
        public int Nivel { get; set; }

        [Required(ErrorMessage = "TipoCategoria é obrigatório")]
        [StringLength(50)]
        public required string TipoCategoria { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Colocacao deve ser positiva")]
        public int Colocacao { get; set; }
    }
}

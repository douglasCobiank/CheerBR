using System.ComponentModel.DataAnnotations;

namespace Cheer.Application.DTOs
{
    public class ChampionshipDto
    {
        public required string Id { get; set; }
        public required string Nome { get; set; }
    }

    public class CreateChampionshipDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(200, ErrorMessage = "Nome não pode exceder 200 caracteres")]
        public required string Nome { get; set; }
    }
}

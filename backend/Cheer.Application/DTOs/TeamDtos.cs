using System.Collections.Generic;

namespace Cheer.Application.DTOs
{
    public class TeamDto
    {
        public required string Id { get; set; }
        public required string Nome { get; set; }
        public string? Programa { get; set; }
        public int? Nivel { get; set; }
        public required string Cidade { get; set; }
        public required string Estado { get; set; }
        public required string Categoria { get; set; }
        public string? Instagram { get; set; }
        public string? Facebook { get; set; }
        public string? Coach { get; set; }
        public string? Fundacao { get; set; }
        public required string Status { get; set; }
        public string? LogoUrl { get; set; }
        public int Score { get; set; }
        public List<CompetitionResultDto> Results { get; set; } = new();
    }

    public class UpdateTeamDto : CreateTeamDto
    {
        public required string Id { get; set; }
    }
}
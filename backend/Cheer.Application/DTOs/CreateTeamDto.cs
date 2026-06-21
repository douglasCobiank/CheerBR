using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cheer.Application.DTOs
{
    public class CreateTeamDto
    {
        public required string Nome { get; set; }
        public string? Programa { get; set; }
        public int? Nivel { get; set; }
        public required string Cidade { get; set; }
        public string Estado { get; set; } = "PR";
        public required string Categoria { get; set; }
        public string? Instagram { get; set; }
        public string? Facebook { get; set; }
        public string? Coach { get; set; }
        public string? Fundacao { get; set; }
        public required string Status { get; set; }
        public string? LogoUrl { get; set; }
    }
}
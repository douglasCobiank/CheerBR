using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cheer.Application.DTOs
{
    public class CreateCompetitionResultDto
    {
        public int Ano { get; set; }
        public required string NomeCampeonato { get; set; }
        public required string Importancia { get; set; }
        public int Nivel { get; set; }
        public required string TipoCategoria { get; set; }
        public int Colocacao { get; set; }
    }
}
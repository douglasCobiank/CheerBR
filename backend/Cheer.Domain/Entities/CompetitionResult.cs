using System;

namespace Cheer.Domain.Entities
{
    public class CompetitionResult
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string TeamId { get; set; }
        public int Ano { get; set; }
        public required string NomeCampeonato { get; set; }
        public required string Importancia { get; set; }
        public int Nivel { get; set; }
        public required string TipoCategoria { get; set; }
        public int Colocacao { get; set; }

        public Team? Team { get; set; }
    }
}

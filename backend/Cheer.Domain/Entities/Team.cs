using System;
using System.Collections.Generic;
using System.Linq;

namespace Cheer.Domain.Entities
{
    public class Team
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Nome { get; set; }
        public string? Programa { get; set; }
        public int? Nivel { get; set; } // O nível geral da equipe não interfere mais no score
        public required string Cidade { get; set; }
        public string Estado { get; set; } = "PR";
        public required string Categoria { get; set; }
        public string? Instagram { get; set; }
        public string? Facebook { get; set; }
        public string? Coach { get; set; }
        public string? Fundacao { get; set; }
        public required string Status { get; set; }
        public string? LogoUrl { get; set; }
        
        public int Score { get; set; }

        public ICollection<CompetitionResult> Results { get; set; } = new List<CompetitionResult>();

        public void CalculateScore(int currentYear)
        {
            if (Results == null || !Results.Any())
            {
                Score = 0;
                return;
            }

            double totalScore = 0;

            foreach (var r in Results)
            {
                double colocacaoPts = r.Colocacao switch
                {
                    1 => 100.0,
                    2 => 70.0,
                    3 => 50.0,
                    4 => 30.0,
                    5 => 20.0,
                    _ => 10.0
                };

                double importanciaPts = r.Importancia switch
                {
                    "Internacional" => 3.0,
                    "Nacional" => 2.5,
                    "Estadual" => 2.0,
                    "Regional" => 1.7,
                    "Municipal" => 1.5,
                    _ => 1.0 // fallback
                };

                double nivelPts = r.Nivel switch
                {
                    1 => 1.1,
                    2 => 1.2,
                    3 => 1.3,
                    4 => 1.4,
                    5 => 1.5,
                    _ => 1.0 // fallback
                };

                double categoriaPts = r.TipoCategoria switch
                {
                    "Team Cheer" => 1.5,
                    "Categoria de Grupo" => 1.2,
                    "Partner / Duplas" => 1.1,
                    "Skills Individuais" => 0.9,
                    _ => 1.0 // fallback
                };

                int diffAnos = Math.Max(0, currentYear - r.Ano);
                double pesoAno = Math.Max(0.0, 1.0 - (diffAnos * 0.1));

                double resultScore = colocacaoPts * importanciaPts * nivelPts * categoriaPts * pesoAno;
                totalScore += resultScore;
            }

            Score = (int)Math.Round(totalScore);
        }
    }
}

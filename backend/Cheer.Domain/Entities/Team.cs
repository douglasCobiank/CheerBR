using Cheer.Domain.Constants;

namespace Cheer.Domain.Entities
{
    public class Team
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
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

        public int Score { get; set; }

        public ICollection<CompetitionResult> Results { get; set; } = new List<CompetitionResult>();

        public void CalculateScore(int currentYear)
        {
            if (Results.Count == 0)
            {
                Score = 0;
                return;
            }

            double totalScore = 0;

            foreach (var r in Results)
            {
                double colocacaoPts = ScoreConstants.PlacementPoints.GetValueOrDefault(
                    r.Colocacao, ScoreConstants.DefaultPlacementPoints);

                double importanciaPts = ScoreConstants.ImportanceWeights.GetValueOrDefault(
                    r.Importancia, ScoreConstants.DefaultImportanceWeight);

                double categoriaPts = ScoreConstants.CategoryWeights.GetValueOrDefault(
                    r.TipoCategoria, ScoreConstants.DefaultCategoryWeight);

                int diffAnos = Math.Max(0, currentYear - r.Ano);
                double pesoAno = Math.Max(ScoreConstants.MinDecay, 1.0 - diffAnos * ScoreConstants.YearDecayRate);

                totalScore += colocacaoPts * importanciaPts * categoriaPts * pesoAno;
            }

            Score = (int)Math.Round(totalScore);
        }
    }
}

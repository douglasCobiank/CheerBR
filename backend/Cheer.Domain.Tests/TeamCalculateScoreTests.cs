using Cheer.Domain.Constants;
using Cheer.Domain.Entities;
using Xunit;

namespace Cheer.Domain.Tests;

/// <summary>
/// Testes golden do algoritmo de pontuacao ProCheer (CalculateScore).
///
/// O artefato destes testes e duplo:
///   1. Proteger a fórmula contra regressões (qualquer mudanca nos pesos
///      quebra um teste e obriga a atualizar deliberadamente).
///   2. Documentar a fórmula oficial (spec do README):
///        score = Σ colocacao_pts * importancia * nivel * categoria * decay
///      decay = max(0, 1 - (ano_atual - ano_resultado) * 0.1)
///
/// Bug que motivou este teste: `r.Nivel` era ignorado na fórmula e
/// `ScoreConstants.LevelWeights` estava morto. Veja commit que introduz
/// este arquivo.
/// </summary>
public class TeamCalculateScoreTests
{
    private const int CurrentYear = 2026;

    private static Team MakeTeam() => new()
    {
        Nome = "T",
        Cidade = "C",
        Estado = "PR",
        Categoria = "Team Cheer",
        Status = "Ativo",
    };

    private static CompetitionResult MakeResult(
        int ano = CurrentYear,
        int colocacao = 1,
        string importancia = "Nacional",
        int nivel = 4,
        string tipoCategoria = "Team Cheer") => new()
        {
            TeamId = "team-1",
            Ano = ano,
            NomeCampeonato = "X",
            Importancia = importancia,
            Nivel = nivel,
            TipoCategoria = tipoCategoria,
            Colocacao = colocacao,
        };

    [Fact]
    public void Team_sem_results_tem_score_zero()
    {
        var team = MakeTeam();
        team.CalculateScore(CurrentYear);
        Assert.Equal(0, team.Score);
    }

    [Fact]
    public void Result_do_ano_atual_usa_decay_1()
    {
        // Colocacao 1 (100 pts) * Nacional (2.5) * Nivel 4 (1.4) * Team Cheer (1.5) * decay 1.0
        // = 100 * 2.5 * 1.4 * 1.5 = 525
        var team = MakeTeam();
        team.Results.Add(MakeResult(ano: CurrentYear, colocacao: 1, importancia: "Nacional", nivel: 4, tipoCategoria: "Team Cheer"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(525, team.Score);
    }

    [Fact]
    public void Result_de_5_anos_atras_usa_decay_0_5()
    {
        // decay = 1 - (2026 - 2021) * 0.1 = 0.5
        // 100 * 2.5 * 1.4 * 1.5 * 0.5 = 262.5 -> Math.Round (banker's: .5 -> par) = 262
        var team = MakeTeam();
        team.Results.Add(MakeResult(ano: CurrentYear - 5, colocacao: 1, importancia: "Nacional", nivel: 4, tipoCategoria: "Team Cheer"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(262, team.Score);
    }

    [Fact]
    public void Result_de_10_anos_atras_usa_decay_zero_e_nao_contriui()
    {
        // decay = 1 - 10 * 0.1 = 0 -> resultado "expira"
        var team = MakeTeam();
        team.Results.Add(MakeResult(ano: CurrentYear - 10, colocacao: 1, importancia: "Internacional", nivel: 6, tipoCategoria: "Team Cheer"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(0, team.Score);
    }

    [Fact]
    public void Peso_por_nivel_aumenta_o_score_progressivamente()
    {
        // Mesmo resultado, variando apenas nivel de 1 a 6.
        // Sem peso por nivel (bug antigo), todos os seis teams teriam score igual.
        var scores = new List<int>();
        for (var nivel = 1; nivel <= 6; nivel++)
        {
            var team = MakeTeam();
            team.Results.Add(MakeResult(colocacao: 1, importancia: "Nacional", nivel: nivel, tipoCategoria: "Team Cheer"));
            team.CalculateScore(CurrentYear);
            scores.Add(team.Score);
        }

        // 100 * 2.5 * nivel * 1.5 = 375 * nivel, com Math.Round (banker's: .5 -> par)
        //   nivel 1: 412.5 -> 412
        //   nivel 2: 450
        //   nivel 3: 487.5 -> 488
        //   nivel 4: 525
        //   nivel 5: 562.5 -> 562
        //   nivel 6: 600
        Assert.Equal(new[] { 412, 450, 488, 525, 562, 600 }, scores);
        // Critico: confirmar que niveis diferentes produzem scores diferentes
        Assert.NotEqual(scores[0], scores[5]); // regressao do bug teria feito isso falhar
    }

    [Fact]
    public void Colocacao_4_usa_pontos_base_30()
    {
        // 30 * 2.5 * 1.4 * 1.5 = 157.5 -> 158
        var team = MakeTeam();
        team.Results.Add(MakeResult(colocacao: 4, importancia: "Nacional", nivel: 4, tipoCategoria: "Team Cheer"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(158, team.Score);
    }

    [Fact]
    public void Colocacao_acima_de_5_usa_default_10_pts()
    {
        // Colocacao 8 -> 10 pts (DefaultPlacementPoints)
        // 10 * 2.5 * 1.4 * 1.5 = 52.5 -> Math.Round (banker's) = 52
        var team = MakeTeam();
        team.Results.Add(MakeResult(colocacao: 8, importancia: "Nacional", nivel: 4, tipoCategoria: "Team Cheer"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(52, team.Score);
    }

    [Fact]
    public void Importance_desconhecida_cai_para_default_1()
    {
        // "Foo" -> DefaultImportanceWeight = 1.0
        // 100 * 1.0 * 1.4 * 1.5 = 210
        var team = MakeTeam();
        team.Results.Add(MakeResult(colocacao: 1, importancia: "Foo", nivel: 4, tipoCategoria: "Team Cheer"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(210, team.Score);
    }

    [Fact]
    public void Categoria_desconhecida_cai_para_default_1()
    {
        // 100 * 2.5 * 1.4 * 1.0 = 350
        var team = MakeTeam();
        team.Results.Add(MakeResult(colocacao: 1, importancia: "Nacional", nivel: 4, tipoCategoria: "Inexistente"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(350, team.Score);
    }

    [Fact]
    public void Nivel_desconhecido_cai_para_default_1()
    {
        // 100 * 2.5 * 1.0 * 1.5 = 375 (Nivel 0 ou 100 -> DefaultLevelWeight = 1.0)
        var team = MakeTeam();
        team.Results.Add(MakeResult(colocacao: 1, importancia: "Nacional", nivel: 0, tipoCategoria: "Team Cheer"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(375, team.Score);
    }

    [Fact]
    public void Multiplos_results_somam()
    {
        // Result 1: 100 * 2.5 * 1.4 * 1.5 * 1.0 = 525
        // Result 2 (ano -5): 70 * 2.0 * 2.0 * 0.5 = 140 -> Estadual=2.0, nivel=2=1.2
        //    Erro: 70 * 2.0 * 1.2 * 1.5 * 0.5 = 126
        // Recalc Result 2: colocacao 2 (70) * Estadual (2.0) * nivel 2 (1.2) * Team Cheer (1.5) * decay 0.5
        //   = 70 * 2.0 * 1.2 * 1.5 * 0.5 = 126
        // Total = 525 + 126 = 651
        var team = MakeTeam();
        team.Results.Add(MakeResult(ano: CurrentYear, colocacao: 1, importancia: "Nacional", nivel: 4, tipoCategoria: "Team Cheer"));
        team.Results.Add(MakeResult(ano: CurrentYear - 5, colocacao: 2, importancia: "Estadual", nivel: 2, tipoCategoria: "Team Cheer"));
        team.CalculateScore(CurrentYear);
        Assert.Equal(651, team.Score);
    }

    [Fact]
    public void Bug_regressao_nivel_ignorado_nao_voltou()
    {
        // Antes do fix: nivel 1 e nivel 6 produziam o mesmo score.
        // Depois do fix: nivel 6 score e 1.6/1.1 = ~1.4545x maior.
        var teamNivel1 = MakeTeam();
        teamNivel1.Results.Add(MakeResult(colocacao: 1, importancia: "Nacional", nivel: 1, tipoCategoria: "Team Cheer"));
        teamNivel1.CalculateScore(CurrentYear);

        var teamNivel6 = MakeTeam();
        teamNivel6.Results.Add(MakeResult(colocacao: 1, importancia: "Nacional", nivel: 6, tipoCategoria: "Team Cheer"));
        teamNivel6.CalculateScore(CurrentYear);

        Assert.True(teamNivel6.Score > teamNivel1.Score, "Nivel 6 deve pontuar mais que nivel 1");
        Assert.NotEqual(teamNivel1.Score, teamNivel6.Score);
    }
}

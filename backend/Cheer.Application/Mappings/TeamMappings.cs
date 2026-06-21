using Cheer.Application.DTOs;
using Cheer.Domain.Entities;

namespace Cheer.Application.Mappings;

public static class TeamMappings
{
    public static TeamDto ToDto(this Team team)
    {
        return new TeamDto
        {
            Id = team.Id,
            Nome = team.Nome,
            Programa = team.Programa,
            Nivel = team.Nivel,
            Cidade = team.Cidade,
            Estado = team.Estado,
            Categoria = team.Categoria,
            Instagram = team.Instagram,
            Facebook = team.Facebook,
            Coach = team.Coach,
            Fundacao = team.Fundacao,
            Status = team.Status,
            LogoUrl = team.LogoUrl,
            Score = team.Score,
            Results = team.Results?
                .Select(r => r.ToDto())
                .ToList() ?? [],
        };
    }

    public static CompetitionResultDto ToDto(this CompetitionResult result)
    {
        return new CompetitionResultDto
        {
            Id = result.Id,
            TeamId = result.TeamId,
            Ano = result.Ano,
            NomeCampeonato = result.NomeCampeonato,
            Importancia = result.Importancia,
            Nivel = result.Nivel,
            TipoCategoria = result.TipoCategoria,
            Colocacao = result.Colocacao,
        };
    }

    public static Team ToEntity(this CreateTeamDto dto)
    {
        return new Team
        {
            Nome = dto.Nome,
            Programa = dto.Programa,
            Nivel = dto.Nivel,
            Cidade = dto.Cidade,
            Estado = dto.Estado,
            Categoria = dto.Categoria,
            Instagram = dto.Instagram,
            Facebook = dto.Facebook,
            Coach = dto.Coach,
            Fundacao = dto.Fundacao,
            Status = dto.Status,
            LogoUrl = dto.LogoUrl,
        };
    }

    public static void ApplyTo(this UpdateTeamDto dto, Team team)
    {
        team.Nome = dto.Nome;
        team.Programa = dto.Programa;
        team.Nivel = dto.Nivel;
        team.Cidade = dto.Cidade;
        team.Estado = dto.Estado;
        team.Categoria = dto.Categoria;
        team.Instagram = dto.Instagram;
        team.Facebook = dto.Facebook;
        team.Coach = dto.Coach;
        team.Fundacao = dto.Fundacao;
        team.Status = dto.Status;
        team.LogoUrl = dto.LogoUrl;
    }

    public static CompetitionResult ToEntity(this CreateCompetitionResultDto dto, string teamId)
    {
        return new CompetitionResult
        {
            TeamId = teamId,
            Ano = dto.Ano,
            NomeCampeonato = dto.NomeCampeonato,
            Importancia = dto.Importancia,
            Nivel = dto.Nivel,
            TipoCategoria = dto.TipoCategoria,
            Colocacao = dto.Colocacao,
        };
    }

    public static void ApplyTo(this UpdateCompetitionResultDto dto, CompetitionResult result)
    {
        result.Ano = dto.Ano;
        result.NomeCampeonato = dto.NomeCampeonato;
        result.Importancia = dto.Importancia;
        result.Nivel = dto.Nivel;
        result.TipoCategoria = dto.TipoCategoria;
        result.Colocacao = dto.Colocacao;
    }
}

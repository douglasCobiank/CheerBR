using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Cheer.Application.Mappings;
using Cheer.Domain.Interfaces;

namespace Cheer.Application.Services;

public class TeamService : ITeamService
{
    private readonly ITeamRepository _repository;

    public TeamService(ITeamRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TeamDto>> GetTeamsAsync(string? categoria = null, string? cidade = null, string? q = null, int? nivel = null)
    {
        var teams = await _repository.GetAllAsync(categoria, cidade, q, nivel);
        var currentYear = DateTime.Now.Year;
        foreach (var t in teams) t.CalculateScore(currentYear);

        return teams.Select(t => t.ToDto());
    }

    public async Task<TeamDto?> GetTeamByIdAsync(string id)
    {
        var team = await _repository.GetByIdAsync(id);
        if (team == null) return null;

        team.CalculateScore(DateTime.Now.Year);
        return team.ToDto();
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto)
    {
        var team = dto.ToEntity();
        team.CalculateScore(DateTime.Now.Year);
        var created = await _repository.AddAsync(team);
        return created.ToDto();
    }

    public async Task UpdateTeamAsync(UpdateTeamDto dto)
    {
        var team = await _repository.GetByIdAsync(dto.Id)
            ?? throw new InvalidOperationException("Team not found");

        dto.ApplyTo(team);
        team.CalculateScore(DateTime.Now.Year);
        await _repository.UpdateAsync(team);
    }

    public async Task DeleteTeamAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<CompetitionResultDto> AddResultAsync(string teamId, CreateCompetitionResultDto dto)
    {
        var team = await _repository.GetByIdAsync(teamId)
            ?? throw new InvalidOperationException("Team not found");

        var result = dto.ToEntity(teamId);
        result.Team = team;
        team.Results.Add(result);
        team.CalculateScore(DateTime.Now.Year);

        await _repository.UpdateAsync(team);
        return result.ToDto();
    }

    public async Task<CompetitionResultDto> UpdateResultAsync(string teamId, string resultId, UpdateCompetitionResultDto dto)
    {
        var team = await _repository.GetByIdAsync(teamId)
            ?? throw new InvalidOperationException("Team not found");

        var result = team.Results.FirstOrDefault(r => r.Id == resultId)
            ?? throw new InvalidOperationException("Result not found");

        dto.ApplyTo(result);
        team.CalculateScore(DateTime.Now.Year);

        await _repository.UpdateAsync(team);
        return result.ToDto();
    }

    public async Task DeleteResultAsync(string teamId, string resultId)
    {
        var team = await _repository.GetByIdAsync(teamId)
            ?? throw new InvalidOperationException("Team not found");

        var result = team.Results.FirstOrDefault(r => r.Id == resultId)
            ?? throw new InvalidOperationException("Result not found");

        team.Results.Remove(result);
        team.CalculateScore(DateTime.Now.Year);

        await _repository.UpdateAsync(team);
    }

    public async Task<IEnumerable<TeamDto>> GetRankingAsync(string? categoria = null)
    {
        var ranking = await _repository.GetRankingAsync(categoria);
        var currentYear = DateTime.Now.Year;
        foreach (var t in ranking) t.CalculateScore(currentYear);

        return ranking
            .OrderByDescending(t => t.Score)
            .Select(t => t.ToDto());
    }

    public async Task<StatsOverviewDto> GetStatsOverviewAsync()
    {
        var total = await _repository.GetTotalCountAsync();
        var ativos = await _repository.GetActiveCountAsync();
        var cidades = await _repository.GetCitiesCountAsync();
        var scoreMedio = await _repository.GetAverageScoreAsync();

        var porStatus = await _repository.GetCountsByStatusAsync();
        var porCategoria = await _repository.GetCountsByCategoryAsync();
        var porCidade = await _repository.GetCountsByCityAsync();
        var porNivel = await _repository.GetCountsByLevelAsync();

        return new StatsOverviewDto
        {
            Total = total,
            Ativos = ativos,
            Cidades = cidades,
            ScoreMedio = scoreMedio,
            PorStatus = porStatus.Select(x => new StatItemDto { Name = x.Key, Value = x.Value }).ToList(),
            PorCategoria = porCategoria.Select(x => new StatItemDto { Name = x.Key, Value = x.Value }).ToList(),
            PorCidade = porCidade.Select(x => new StatItemDto { Name = x.Key, Value = x.Value }).ToList(),
            PorNivel = porNivel.Select(x => new StatItemDto { Name = x.Key, Value = x.Value }).ToList(),
        };
    }
}

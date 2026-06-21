using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Cheer.Domain.Entities;
using Cheer.Domain.Interfaces;

namespace Cheer.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _repository;

        public TeamService(ITeamRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TeamDto>> GetTeamsAsync(string? categoria = null, string? cidade = null, string? q = null)
        {
            var teams = await _repository.GetAllAsync(categoria, cidade, q);
            // Dynamic recalculation just to ensure return data is fresh for current year
            var currentYear = DateTime.Now.Year;
            foreach (var t in teams) t.CalculateScore(currentYear);

            return teams.Select(MapToDto);
        }

        public async Task<TeamDto?> GetTeamByIdAsync(string id)
        {
            var team = await _repository.GetByIdAsync(id);
            if (team == null) return null;
            
            team.CalculateScore(DateTime.Now.Year);
            return MapToDto(team);
        }

        public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto)
        {
            var team = new Team
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
                LogoUrl = dto.LogoUrl
            };

            team.CalculateScore(DateTime.Now.Year);
            var createdTeam = await _repository.AddAsync(team);
            return MapToDto(createdTeam);
        }

        public async Task UpdateTeamAsync(UpdateTeamDto dto)
        {
            var team = await _repository.GetByIdAsync(dto.Id);
            if (team == null) throw new Exception("Team not found");

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

            team.CalculateScore(DateTime.Now.Year);
            await _repository.UpdateAsync(team);
        }

        public async Task DeleteTeamAsync(string id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<CompetitionResultDto> AddResultAsync(string teamId, CreateCompetitionResultDto dto)
        {
            var team = await _repository.GetByIdAsync(teamId);
            if (team == null) throw new Exception("Team not found");

            var result = new CompetitionResult
            {
                TeamId = teamId,
                Ano = dto.Ano,
                NomeCampeonato = dto.NomeCampeonato,
                Importancia = dto.Importancia,
                Nivel = dto.Nivel,
                TipoCategoria = dto.TipoCategoria,
                Colocacao = dto.Colocacao,
                Team = team
            };

            team.Results.Add(result);
            team.CalculateScore(DateTime.Now.Year);
            
            await _repository.UpdateAsync(team); // This will save the new result and the new score

            return new CompetitionResultDto
            {
                Id = result.Id,
                TeamId = result.TeamId,
                Ano = result.Ano,
                NomeCampeonato = result.NomeCampeonato,
                Importancia = result.Importancia,
                Nivel = result.Nivel,
                TipoCategoria = result.TipoCategoria,
                Colocacao = result.Colocacao
            };
        }

        public async Task<IEnumerable<TeamDto>> GetRankingAsync(string? categoria = null)
        {
            var ranking = await _repository.GetRankingAsync(categoria);
            // Make sure the ranking is fresh
            var currentYear = DateTime.Now.Year;
            foreach (var t in ranking) t.CalculateScore(currentYear);
            
            // Re-sort in memory just to be absolutely sure since we recalculated
            return ranking.OrderByDescending(t => t.Score).Select(MapToDto);
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
                PorNivel = porNivel.Select(x => new StatItemDto { Name = x.Key, Value = x.Value }).ToList()
            };
        }

        private static TeamDto MapToDto(Team team)
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
                Results = team.Results?.Select(r => new CompetitionResultDto
                {
                    Id = r.Id,
                    TeamId = r.TeamId,
                    Ano = r.Ano,
                    NomeCampeonato = r.NomeCampeonato,
                    Importancia = r.Importancia,
                    Nivel = r.Nivel,
                    TipoCategoria = r.TipoCategoria,
                    Colocacao = r.Colocacao
                }).ToList() ?? new List<CompetitionResultDto>()
            };
        }
    }
}

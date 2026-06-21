using System.Collections.Generic;
using System.Threading.Tasks;
using Cheer.Application.DTOs;

namespace Cheer.Application.Interfaces
{
    public interface ITeamService
    {
        Task<IEnumerable<TeamDto>> GetTeamsAsync(string? categoria = null, string? cidade = null, string? q = null);
        Task<TeamDto?> GetTeamByIdAsync(string id);
        Task<TeamDto> CreateTeamAsync(CreateTeamDto dto);
        Task UpdateTeamAsync(UpdateTeamDto dto);
        Task DeleteTeamAsync(string id);
        Task<CompetitionResultDto> AddResultAsync(string teamId, CreateCompetitionResultDto dto);
        Task<IEnumerable<TeamDto>> GetRankingAsync(string? categoria = null);
        Task<StatsOverviewDto> GetStatsOverviewAsync();
    }
}

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cheer.Application.DTOs;

namespace Cheer.Application.Interfaces
{
    public interface ITeamService
    {
        Task<(IEnumerable<TeamDto> items, int total)> GetTeamsAsync(int page, int pageSize,
            string? categoria = null, string? cidade = null, string? q = null, int? nivel = null);
        Task<TeamDto?> GetTeamByIdAsync(string id);
        Task<TeamDto> CreateTeamAsync(CreateTeamDto dto);
        Task UpdateTeamAsync(UpdateTeamDto dto);
        Task DeleteTeamAsync(string id);
        Task<CompetitionResultDto> AddResultAsync(string teamId, CreateCompetitionResultDto dto);
        Task<CompetitionResultDto> UpdateResultAsync(string teamId, string resultId, UpdateCompetitionResultDto dto);
        Task DeleteResultAsync(string teamId, string resultId);
        Task<IEnumerable<TeamDto>> GetRankingAsync(string? categoria = null);
        Task<StatsOverviewDto> GetStatsOverviewAsync();

        // Upload de logo: valida tipo/extensao, grava em disco com nome seguro,
        // atualiza apenas a coluna LogoUrl (sem sobrescrever os outros campos do Team
        // a partir de um snapshot stale lido do DB) e remove o logo anterior.
        // Retorna a URL absoluta construida a partir de `schemeHost` + path.
        Task<string> SetLogoAsync(string id, Stream content, string contentType, string originalFileName);
    }
}

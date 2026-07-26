using System.Collections.Generic;
using System.Threading.Tasks;
using Cheer.Domain.Entities;

namespace Cheer.Domain.Interfaces
{
    public interface ITeamRepository
    {
        // Retorna total de teams (sem paginacao) para os filtros dados
        Task<int> GetCountAsync(string? categoria = null, string? cidade = null, string? q = null, int? nivel = null);
        // Paginacao: page 1-based, pageSize default 50, max 200
        Task<(IEnumerable<Team> items, int total)> GetPagedAsync(int page, int pageSize,
            string? categoria = null, string? cidade = null, string? q = null, int? nivel = null);
        Task<Team?> GetByIdAsync(string id);
        Task<Team> AddAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(string id); // soft-delete: marca IsDeleted=true
        Task<IEnumerable<Team>> GetRankingAsync(string? categoria = null);

        Task UpdateLogoUrlAsync(string id, string? logoUrl);

        Task<int> GetTotalCountAsync();
        Task<int> GetActiveCountAsync();
        Task<int> GetCitiesCountAsync();
        Task<double> GetAverageScoreAsync();
        Task<IEnumerable<KeyValuePair<string, int>>> GetCountsByStatusAsync();
        Task<IEnumerable<KeyValuePair<string, int>>> GetCountsByCategoryAsync();
        Task<IEnumerable<KeyValuePair<string, int>>> GetCountsByCityAsync();
        Task<IEnumerable<KeyValuePair<string, int>>> GetCountsByLevelAsync();
    }
}

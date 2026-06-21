using System.Collections.Generic;
using System.Threading.Tasks;
using Cheer.Domain.Entities;

namespace Cheer.Domain.Interfaces
{
    public interface ITeamRepository
    {
        Task<IEnumerable<Team>> GetAllAsync(string? categoria = null, string? cidade = null, string? q = null);
        Task<Team?> GetByIdAsync(string id);
        Task<Team> AddAsync(Team team);
        Task UpdateAsync(Team team);
        Task DeleteAsync(string id);
        Task<IEnumerable<Team>> GetRankingAsync(string? categoria = null);
        
        // For stats overview
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

using System.Collections.Generic;
using System.Threading.Tasks;
using Cheer.Domain.Entities;

namespace Cheer.Domain.Interfaces
{
    public interface IChampionshipRepository
    {
        Task<IEnumerable<Championship>> GetAllAsync();
        Task<Championship?> GetByIdAsync(string id);
        Task<Championship> AddAsync(Championship championship);
        Task UpdateAsync(Championship championship);
        Task DeleteAsync(string id);
    }
}

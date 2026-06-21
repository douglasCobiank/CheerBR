using System.Collections.Generic;
using System.Threading.Tasks;
using Cheer.Application.DTOs;

namespace Cheer.Application.Interfaces
{
    public interface IChampionshipService
    {
        Task<IEnumerable<ChampionshipDto>> GetAllAsync();
        Task<ChampionshipDto> CreateAsync(CreateChampionshipDto dto);
        Task UpdateAsync(string id, CreateChampionshipDto dto);
        Task DeleteAsync(string id);
    }
}

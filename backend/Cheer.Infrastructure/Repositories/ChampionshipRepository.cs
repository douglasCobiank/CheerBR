using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheer.Domain.Entities;
using Cheer.Domain.Interfaces;
using Cheer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cheer.Infrastructure.Repositories
{
    public class ChampionshipRepository : IChampionshipRepository
    {
        private readonly AppDbContext _context;

        public ChampionshipRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Championship>> GetAllAsync()
        {
            return await _context.Championships.OrderBy(c => c.Nome).ToListAsync();
        }

        public async Task<Championship?> GetByIdAsync(string id)
        {
            return await _context.Championships.FindAsync(id);
        }

        public async Task<Championship> AddAsync(Championship championship)
        {
            _context.Championships.Add(championship);
            await _context.SaveChangesAsync();
            return championship;
        }

        public async Task UpdateAsync(Championship championship)
        {
            _context.Championships.Update(championship);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var championship = await _context.Championships.FindAsync(id);
            if (championship != null)
            {
                _context.Championships.Remove(championship);
                await _context.SaveChangesAsync();
            }
        }
    }
}

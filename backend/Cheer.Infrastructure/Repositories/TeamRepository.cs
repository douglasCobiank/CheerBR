using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cheer.Domain.Entities;
using Cheer.Domain.Interfaces;
using Cheer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cheer.Infrastructure.Repositories
{
    public class TeamRepository : ITeamRepository
    {
        private readonly AppDbContext _context;

        public TeamRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Team>> GetAllAsync(string? categoria = null, string? cidade = null, string? q = null)
        {
            var query = _context.Teams.Include(t => t.Results).AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(t => t.Categoria == categoria);

            if (!string.IsNullOrWhiteSpace(cidade))
                query = query.Where(t => t.Cidade == cidade);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => EF.Functions.Like(t.Nome, $"%{q}%") || EF.Functions.Like(t.Programa, $"%{q}%"));

            return await query.ToListAsync();
        }

        public async Task<Team?> GetByIdAsync(string id)
        {
            return await _context.Teams.Include(t => t.Results).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Team> AddAsync(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Team>> GetRankingAsync(string? categoria = null)
        {
            var query = _context.Teams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(t => t.Categoria == categoria);

            return await query.OrderByDescending(t => t.Score).ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Teams.CountAsync();
        }

        public async Task<int> GetActiveCountAsync()
        {
            return await _context.Teams.CountAsync(t => t.Status == "Ativo");
        }

        public async Task<int> GetCitiesCountAsync()
        {
            return await _context.Teams.Select(t => t.Cidade).Distinct().CountAsync();
        }

        public async Task<double> GetAverageScoreAsync()
        {
            if (!await _context.Teams.AnyAsync()) return 0;
            return await _context.Teams.AverageAsync(t => t.Score);
        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> GetCountsByStatusAsync()
        {
            return await _context.Teams
                .GroupBy(t => t.Status)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToListAsync();
        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> GetCountsByCategoryAsync()
        {
            return await _context.Teams
                .GroupBy(t => t.Categoria)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToListAsync();
        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> GetCountsByCityAsync()
        {
            return await _context.Teams
                .GroupBy(t => t.Cidade)
                .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .ToListAsync();
        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> GetCountsByLevelAsync()
        {
            return await _context.Teams
                .Where(t => t.Nivel != null)
                .GroupBy(t => t.Nivel.ToString()!)
                .Select(g => new KeyValuePair<string, int>("Nível " + g.Key, g.Count()))
                .ToListAsync();
        }
    }
}

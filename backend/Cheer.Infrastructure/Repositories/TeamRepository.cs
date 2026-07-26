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

        public async Task<int> GetCountAsync(string? categoria = null, string? cidade = null, string? q = null, int? nivel = null)
        {
            var query = _context.Teams.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(t => t.Categoria == categoria);
            if (!string.IsNullOrWhiteSpace(cidade))
                query = query.Where(t => t.Cidade == cidade);
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t =>
                    EF.Functions.ILike(t.Nome, $"%{q}%")
                    || (t.Programa != null && EF.Functions.ILike(t.Programa, $"%{q}%")));
            if (nivel.HasValue)
                query = query.Where(t => t.Nivel == nivel.Value);

            return await query.CountAsync();
        }

        public async Task<(IEnumerable<Team> items, int total)> GetPagedAsync(int page, int pageSize,
            string? categoria = null, string? cidade = null, string? q = null, int? nivel = null)
        {
            var query = _context.Teams
                .Include(t => t.Results)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(t => t.Categoria == categoria);
            if (!string.IsNullOrWhiteSpace(cidade))
                query = query.Where(t => t.Cidade == cidade);
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t =>
                    EF.Functions.ILike(t.Nome, $"%{q}%")
                    || (t.Programa != null && EF.Functions.ILike(t.Programa, $"%{q}%")));
            if (nivel.HasValue)
                query = query.Where(t => t.Nivel == nivel.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.Nome)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Team?> GetByIdAsync(string id)
        {
            return await _context.Teams
                .Include(t => t.Results)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
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
                team.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Team>> GetRankingAsync(string? categoria = null)
        {
            var query = _context.Teams
                .Include(t => t.Results)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(t => t.Categoria == categoria);

            return await query.OrderByDescending(t => t.Score).ToListAsync();
        }

        public async Task UpdateLogoUrlAsync(string id, string? logoUrl)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team is null) return;
            team.LogoUrl = logoUrl;
            _context.Entry(team).Property(t => t.LogoUrl).IsModified = true;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Teams.IgnoreQueryFilters().CountAsync();
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

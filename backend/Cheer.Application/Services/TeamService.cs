using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Cheer.Application.Mappings;
using Cheer.Domain.Exceptions;
using Cheer.Domain.Interfaces;

namespace Cheer.Application.Services;

public class TeamService : ITeamService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };
    private const long MaxLogoBytes = 5_000_000;

    private readonly ITeamRepository _repository;

    public TeamService(ITeamRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TeamDto>> GetTeamsAsync(string? categoria = null, string? cidade = null, string? q = null, int? nivel = null)
    {
        var teams = await _repository.GetAllAsync(categoria, cidade, q, nivel);
        var currentYear = DateTime.Now.Year;
        foreach (var t in teams) t.CalculateScore(currentYear);

        return teams.Select(t => t.ToDto());
    }

    public async Task<TeamDto?> GetTeamByIdAsync(string id)
    {
        var team = await _repository.GetByIdAsync(id);
        if (team == null) return null;

        team.CalculateScore(DateTime.Now.Year);
        return team.ToDto();
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto)
    {
        var team = dto.ToEntity();
        team.CalculateScore(DateTime.Now.Year);
        var created = await _repository.AddAsync(team);
        return created.ToDto();
    }

    public async Task UpdateTeamAsync(UpdateTeamDto dto)
    {
        var team = await _repository.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("Team", dto.Id);

        dto.ApplyTo(team);
        team.CalculateScore(DateTime.Now.Year);
        await _repository.UpdateAsync(team);
    }

    public async Task DeleteTeamAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<CompetitionResultDto> AddResultAsync(string teamId, CreateCompetitionResultDto dto)
    {
        var team = await _repository.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Team", teamId);

        var result = dto.ToEntity(teamId);
        result.Team = team;
        team.Results.Add(result);
        team.CalculateScore(DateTime.Now.Year);

        await _repository.UpdateAsync(team);
        return result.ToDto();
    }

    public async Task<CompetitionResultDto> UpdateResultAsync(string teamId, string resultId, UpdateCompetitionResultDto dto)
    {
        var team = await _repository.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Team", teamId);

        var result = team.Results.FirstOrDefault(r => r.Id == resultId)
            ?? throw new NotFoundException("CompetitionResult", resultId);

        dto.ApplyTo(result);
        team.CalculateScore(DateTime.Now.Year);

        await _repository.UpdateAsync(team);
        return result.ToDto();
    }

    public async Task DeleteResultAsync(string teamId, string resultId)
    {
        var team = await _repository.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Team", teamId);

        var result = team.Results.FirstOrDefault(r => r.Id == resultId)
            ?? throw new NotFoundException("CompetitionResult", resultId);

        team.Results.Remove(result);
        team.CalculateScore(DateTime.Now.Year);

        await _repository.UpdateAsync(team);
    }

    public async Task<IEnumerable<TeamDto>> GetRankingAsync(string? categoria = null)
    {
        var ranking = await _repository.GetRankingAsync(categoria);
        var currentYear = DateTime.Now.Year;
        foreach (var t in ranking) t.CalculateScore(currentYear);

        return ranking
            .OrderByDescending(t => t.Score)
            .Select(t => t.ToDto());
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
            PorNivel = porNivel.Select(x => new StatItemDto { Name = x.Key, Value = x.Value }).ToList(),
        };
    }

    public async Task<string> SetLogoAsync(string id, Stream content, string contentType, string originalFileName, string schemeHost)
    {
        // Validacoes basicas de tamanho/tipo antes de tocar o disco
        if (content == null || content.Length == 0)
            throw new ArgumentException("Arquivo vazio.");
        if (content.Length > MaxLogoBytes)
            throw new ArgumentException("Logo nao pode exceder 5 MB.");

        var extension = Path.GetExtension(originalFileName);
        if (!AllowedContentTypes.Contains(contentType) || !AllowedExtensions.Contains(extension))
            throw new ArgumentException("Formato de imagem invalido. Aceitos: JPG, PNG, WEBP, GIF.");

        // Confirmar que o team existe (NotFound em vez de criar logo de team inexistente)
        var team = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Team", id);

        // Nome seguro (id + guid + extensao validada). Nao confiar no nome do arquivo do client.
        var safeExtension = AllowedExtensions.First(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase));
        var fileName = $"{id}_{Guid.NewGuid():N}{safeExtension}";

        // Pasta de uploads (env var UPLOADS_PATH configurada em Program.cs; fallback wwwroot/uploads)
        var uploadsFolder = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UPLOADS_PATH"))
            ? Environment.GetEnvironmentVariable("UPLOADS_PATH")!
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await content.CopyToAsync(stream);
        }

        // Remove logo anterior (se houver) — evita crescimento descontrolado do disco
        var previousLogoPath = TryResolveLocalLogoPath(team.LogoUrl, schemeHost);
        if (!string.IsNullOrEmpty(previousLogoPath) && File.Exists(previousLogoPath))
        {
            try { File.Delete(previousLogoPath); } catch { /* best-effort; nao derrubar a operacao */ }
        }

        var logoUrl = $"{schemeHost}/uploads/{fileName}";
        await _repository.UpdateLogoUrlAsync(id, logoUrl);

        return logoUrl;
    }

    private static string? TryResolveLocalLogoPath(string? logoUrl, string schemeHost)
    {
        if (string.IsNullOrWhiteSpace(logoUrl)) return null;
        var prefix = $"{schemeHost}/uploads/";
        if (!logoUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;

        var fileName = logoUrl.Substring(prefix.Length);
        // Rejeitar qualquer path traversal no nome salvo (defensivo — no momento so gravamos GUIDs)
        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\')) return null;

        var uploadsFolder = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UPLOADS_PATH"))
            ? Environment.GetEnvironmentVariable("UPLOADS_PATH")!
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        return Path.Combine(uploadsFolder, fileName);
    }
}

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
    private readonly IStorageService _storage;

    public TeamService(ITeamRepository repository, IStorageService storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<(IEnumerable<TeamDto> items, int total)> GetTeamsAsync(int page, int pageSize,
        string? categoria = null, string? cidade = null, string? q = null, int? nivel = null)
    {
        var (teams, total) = await _repository.GetPagedAsync(page, pageSize, categoria, cidade, q, nivel);
        var currentYear = DateTime.Now.Year;
        foreach (var t in teams) t.CalculateScore(currentYear);

        return (teams.Select(t => t.ToDto()), total);
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

    public async Task<string> SetLogoAsync(string id, Stream content, string contentType, string originalFileName)
    {
        // Validacoes basicas de tamanho/tipo antes de tocar o disco
        if (content == null || content.Length == 0)
            throw new ArgumentException("Arquivo vazio.");
        if (content.Length > MaxLogoBytes)
            throw new ArgumentException("Logo nao pode exceder 5 MB.");

        var extension = Path.GetExtension(originalFileName);
        if (!AllowedContentTypes.Contains(contentType) || !AllowedExtensions.Contains(extension))
            throw new ArgumentException("Formato de imagem invalido. Aceitos: JPG, PNG, WEBP, GIF.");

        // Magic-byte validation: ler primeiros bytes e confirmar que o tipo
        // corresponde ao esperado. Impede ataques de extensao falsa
        // (ex: .png com payload HTML/jar).
        var header = new byte[8];
        var read = await content.ReadAsync(header, 0, header.Length);
        if (read < header.Length || !MatchesMagicBytes(header, extension))
        {
            throw new ArgumentException("Conteudo do arquivo nao corresponde ao formato declarado.");
        }
        content.Seek(0, SeekOrigin.Begin); // voltar para copiar o stream completo

        // Confirmar que o team existe (NotFound em vez de criar logo de team inexistente)
        var team = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Team", id);

        // Nome seguro (id + guid + extensao validada). Nao confiar no nome do arquivo do client.
        var safeExtension = AllowedExtensions.First(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase));
        var fileName = $"{team.Id}_{Guid.NewGuid():N}{safeExtension}";

        // Upload para R2 ou disco local via IStorageService
        var logoUrl = await _storage.UploadAsync(fileName, content, contentType);

        // Remove logo anterior do storage (R2 ou disco)
        var previousKey = _storage.ExtractKey(team.LogoUrl);
        if (previousKey != null)
        {
            await _storage.DeleteAsync(previousKey);
        }

        await _repository.UpdateLogoUrlAsync(id, logoUrl);

        return logoUrl;
    }

    private static bool MatchesMagicBytes(byte[] header, string extension)
    {
        // Headers magicos para tipos de imagem suportados
        // JPEG: FF D8 FF
        if (extension is ".jpg" or ".jpeg")
            return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (extension == ".png")
            return header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E
                && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A
                && header[6] == 0x1A && header[7] == 0x0A;
        // GIF: 47 49 46 38 (39 61 ou 37 61)
        if (extension == ".gif")
            return header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46
                && header[3] == 0x38 && (header[4] == 0x39 || header[4] == 0x37)
                && header[5] == 0x61;
        // WebP: RIFF xxxx WEBP
        if (extension == ".webp")
            return header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46
                && header[3] == 0x46;

        return false;
    }
}

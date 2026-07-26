using Cheer.Api.Auth;
using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Cheer.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Cheer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = ApiKeyAuthorizationExtensions.PolicyName)]
public class TeamsController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(ITeamService teamService, ILogger<TeamsController> logger)
    {
        _teamService = teamService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams(
        [FromQuery] string? categoria,
        [FromQuery] string? cidade,
        [FromQuery] string? q,
        [FromQuery] int? nivel)
    {
        var teams = await _teamService.GetTeamsAsync(categoria, cidade, q, nivel);
        return Ok(teams);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<TeamDto>> GetTeam(string id)
    {
        var team = await _teamService.GetTeamByIdAsync(id);
        if (team == null) return NotFound();
        return Ok(team);
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamDto dto)
    {
        var createdTeam = await _teamService.CreateTeamAsync(dto);
        return CreatedAtAction(nameof(GetTeam), new { id = createdTeam.Id }, createdTeam);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTeam(string id, [FromBody] UpdateTeamDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");
        await _teamService.UpdateTeamAsync(dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTeam(string id)
    {
        await _teamService.DeleteTeamAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/results")]
    public async Task<ActionResult<CompetitionResultDto>> AddResult(string id, [FromBody] CreateCompetitionResultDto dto)
    {
        var result = await _teamService.AddResultAsync(id, dto);
        return Ok(result);
    }

    [HttpPut("{id}/results/{resultId}")]
    public async Task<ActionResult<CompetitionResultDto>> UpdateResult(string id, string resultId, [FromBody] UpdateCompetitionResultDto dto)
    {
        var result = await _teamService.UpdateResultAsync(id, resultId, dto);
        return Ok(result);
    }

    [HttpDelete("{id}/results/{resultId}")]
    public async Task<ActionResult> DeleteResult(string id, string resultId)
    {
        await _teamService.DeleteResultAsync(id, resultId);
        return NoContent();
    }

    [HttpPost("{id}/logo")]
    [RequestSizeLimit(5_000_000)] // 5 MB max no Kestrel
    [EnableRateLimiting("logo")] // protege contra DoS de disco via IP (10 tokens / 20s)
    public async Task<ActionResult> UploadLogo(string id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Validacao expressa no servico; aqui apenas repassamos.
        var request = HttpContext.Request;
        var schemeHost = $"{request.Scheme}://{request.Host}";

        try
        {
            using var stream = file.OpenReadStream();
            var logoUrl = await _teamService.SetLogoAsync(id, stream, file.ContentType, file.FileName, schemeHost);
            return Ok(new { LogoUrl = logoUrl });
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Upload falhou: team nao encontrado");
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Upload rejeitado");
            return BadRequest(ex.Message);
        }
    }
}

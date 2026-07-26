using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cheer.Api.Controllers;

[ApiController]
[Route("api/ranking")]
public class RankingController : ControllerBase
{
    private readonly ITeamService _teamService;

    public RankingController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetRanking([FromQuery] string? categoria)
    {
        var ranking = await _teamService.GetRankingAsync(categoria);
        return Ok(ranking);
    }
}

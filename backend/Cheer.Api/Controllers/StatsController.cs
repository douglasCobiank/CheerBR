using Cheer.Api.Auth;
using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cheer.Api.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsController : ControllerBase
{
    private readonly ITeamService _teamService;

    public StatsController(ITeamService teamService)
    {
        _teamService = teamService;
    }

    [HttpGet("overview")]
    [AllowAnonymous]
    public async Task<ActionResult<StatsOverviewDto>> GetOverview()
    {
        var stats = await _teamService.GetStatsOverviewAsync();
        return Ok(stats);
    }
}

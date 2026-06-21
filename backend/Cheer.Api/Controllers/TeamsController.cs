using System.Collections.Generic;
using System.Threading.Tasks;
using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cheer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams([FromQuery] string? categoria, [FromQuery] string? cidade, [FromQuery] string? q)
        {
            var teams = await _teamService.GetTeamsAsync(categoria, cidade, q);
            return Ok(teams);
        }

        [HttpGet("{id}")]
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
            
            try
            {
                await _teamService.UpdateTeamAsync(dto);
                return NoContent();
            }
            catch
            {
                return NotFound();
            }
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
            try
            {
                var result = await _teamService.AddResultAsync(id, dto);
                return Ok(result);
            }
            catch
            {
                return NotFound("Team not found or error adding result");
            }
        }

        [HttpPut("{id}/results/{resultId}")]
        public async Task<ActionResult<CompetitionResultDto>> UpdateResult(string id, string resultId, [FromBody] UpdateCompetitionResultDto dto)
        {
            try
            {
                var result = await _teamService.UpdateResultAsync(id, resultId, dto);
                return Ok(result);
            }
            catch
            {
                return NotFound("Team or result not found");
            }
        }

        [HttpDelete("{id}/results/{resultId}")]
        public async Task<ActionResult> DeleteResult(string id, string resultId)
        {
            try
            {
                await _teamService.DeleteResultAsync(id, resultId);
                return NoContent();
            }
            catch
            {
                return NotFound("Team or result not found");
            }
        }

        [HttpPost("{id}/logo")]
        public async Task<ActionResult<TeamDto>> UploadLogo(string id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var team = await _teamService.GetTeamByIdAsync(id);
            if (team == null) return NotFound("Team not found");

            // Define the path
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Create a unique filename
            var fileName = $"{id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // The URL to access the file
            var request = HttpContext.Request;
            var logoUrl = $"{request.Scheme}://{request.Host}/uploads/{fileName}";

            // Update team
            var updateDto = new UpdateTeamDto
            {
                Id = team.Id,
                Nome = team.Nome,
                Programa = team.Programa,
                Nivel = team.Nivel,
                Cidade = team.Cidade,
                Estado = team.Estado,
                Categoria = team.Categoria,
                Instagram = team.Instagram,
                Facebook = team.Facebook,
                Coach = team.Coach,
                Fundacao = team.Fundacao,
                Status = team.Status,
                LogoUrl = logoUrl
            };

            await _teamService.UpdateTeamAsync(updateDto);

            return Ok(new { LogoUrl = logoUrl });
        }
    }

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
        public async Task<ActionResult<StatsOverviewDto>> GetOverview()
        {
            var stats = await _teamService.GetStatsOverviewAsync();
            return Ok(stats);
        }
    }
}

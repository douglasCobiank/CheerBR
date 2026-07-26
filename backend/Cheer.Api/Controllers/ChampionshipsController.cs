using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cheer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChampionshipsController : ControllerBase
{
    private readonly IChampionshipService _service;

    public ChampionshipsController(IChampionshipService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChampionshipDto>>> GetAll()
    {
        var championships = await _service.GetAllAsync();
        return Ok(championships);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChampionshipDto>> GetById(string id)
    {
        var championship = await _service.GetByIdAsync(id);
        if (championship == null) return NotFound();
        return Ok(championship);
    }

    [HttpPost]
    public async Task<ActionResult<ChampionshipDto>> Create([FromBody] CreateChampionshipDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] CreateChampionshipDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}

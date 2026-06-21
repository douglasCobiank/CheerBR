using System.Collections.Generic;
using System.Threading.Tasks;
using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cheer.Api.Controllers
{
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

        [HttpPost]
        public async Task<ActionResult<ChampionshipDto>> Create([FromBody] CreateChampionshipDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(string id, [FromBody] CreateChampionshipDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto);
                return NoContent();
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}

using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Cheer.Domain.Entities;
using Cheer.Domain.Interfaces;

namespace Cheer.Application.Services;

public class ChampionshipService : IChampionshipService
{
    private readonly IChampionshipRepository _repository;

    public ChampionshipService(IChampionshipRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ChampionshipDto>> GetAllAsync()
    {
        var championships = await _repository.GetAllAsync();
        return championships.Select(c => new ChampionshipDto
        {
            Id = c.Id,
            Nome = c.Nome,
        });
    }

    public async Task<ChampionshipDto> CreateAsync(CreateChampionshipDto dto)
    {
        var championship = new Championship { Nome = dto.Nome };
        var created = await _repository.AddAsync(championship);
        return new ChampionshipDto
        {
            Id = created.Id,
            Nome = created.Nome,
        };
    }

    public async Task UpdateAsync(string id, CreateChampionshipDto dto)
    {
        var championship = await _repository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Championship not found");
        championship.Nome = dto.Nome;
        await _repository.UpdateAsync(championship);
    }

    public async Task DeleteAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }
}

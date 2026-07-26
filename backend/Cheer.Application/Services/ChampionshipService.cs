using Cheer.Application.DTOs;
using Cheer.Application.Interfaces;
using Cheer.Domain.Entities;
using Cheer.Domain.Exceptions;
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
        return championships.Select(ToDto);
    }

    public async Task<ChampionshipDto?> GetByIdAsync(string id)
    {
        var championship = await _repository.GetByIdAsync(id);
        return championship is null ? null : ToDto(championship);
    }

    public async Task<ChampionshipDto> CreateAsync(CreateChampionshipDto dto)
    {
        var championship = new Championship { Nome = dto.Nome };
        var created = await _repository.AddAsync(championship);
        return ToDto(created);
    }

    public async Task UpdateAsync(string id, CreateChampionshipDto dto)
    {
        var championship = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Championship", id);
        championship.Nome = dto.Nome;
        await _repository.UpdateAsync(championship);
    }

    public async Task DeleteAsync(string id)
    {
        var championship = await _repository.GetByIdAsync(id);
        if (championship is null) return;
        await _repository.DeleteAsync(id);
    }

    private static ChampionshipDto ToDto(Championship c) => new() { Id = c.Id, Nome = c.Nome };
}

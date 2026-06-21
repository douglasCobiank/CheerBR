namespace Cheer.Application.DTOs
{
    public class ChampionshipDto
    {
        public required string Id { get; set; }
        public required string Nome { get; set; }
    }

    public class CreateChampionshipDto
    {
        public required string Nome { get; set; }
    }
}

using System;

namespace Cheer.Domain.Entities
{
    public class Championship
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Nome { get; set; }
    }
}

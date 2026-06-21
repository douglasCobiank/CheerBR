using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cheer.Application.DTOs
{
    public class StatItemDto
    {
        public required string Name { get; set; }
        public int Value { get; set; }
    }
}
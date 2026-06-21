using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cheer.Application.DTOs
{
    public class StatsOverviewDto
    {
        public int Total { get; set; }
        public int Ativos { get; set; }
        public int Cidades { get; set; }
        public double ScoreMedio { get; set; }
        public List<StatItemDto> PorStatus { get; set; } = new();
        public List<StatItemDto> PorCategoria { get; set; } = new();
        public List<StatItemDto> PorCidade { get; set; } = new();
        public List<StatItemDto> PorNivel { get; set; } = new();
    }
}
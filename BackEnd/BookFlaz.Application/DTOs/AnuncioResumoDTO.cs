using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class AnuncioResumoDTO
    {
        public long Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class AvaliacaoDTO
    {
        public int Estrelas { get; set; }
        public string Comentario { get; set; } = string.Empty;
    }
}

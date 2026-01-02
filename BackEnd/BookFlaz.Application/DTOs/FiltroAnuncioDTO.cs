using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class FiltroAnuncioDTO
    {
        public string? TermoPesquisa { get; set; }
        public long? CategoriaId { get; set; }
        public decimal? PrecoMinimo { get; set; }
        public decimal? PrecoMaximo { get; set; }
        public EstadoLivro? EstadoLivro { get; set; }
        public TipoAnuncio? TipoAnuncio { get; set; }
    }
}

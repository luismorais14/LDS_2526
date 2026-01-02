using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class PontosMovimentadosDTO
    {
        public int Quantidade { get; set; }
        public DateTime Data { get; set; }
        public long? AnuncioId { get; set; }
        public string? ImagemAnuncio { get; set; }
        public string? TituloAnuncio { get; set; }
        public decimal? Preco { get; set; }
        public long? ConversaId { get; set; }
        public TipoMovimento TipoMovimento { get; set; }
    }
}

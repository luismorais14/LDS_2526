using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Domain.Enums;

namespace BookFlaz.Application.DTOs
{
	public class TransacaoResumoDTO
	{
		public long Id { get; set; }
		public DateTime Data { get; set; }
		public String Estado { get; set; } = string.Empty;
		public long PedidoId { get; set; }

		public long? AnuncioId { get; set; }
		public String? TituloAnuncio { get; set; }
		public string? ImagemAnuncio { get; set; }
		public decimal? Preco { get; set; }
		public TipoAnuncio? TipoAnuncio { get; set; }
		public long OutroUtilizadorId { get; set; }
		public decimal ValorFinal { get; set; }
		public int PontosUsados { get; set; }
		public decimal ValorDesconto { get; set; }

		public string Papel { get; set; } = string.Empty;

	}
}

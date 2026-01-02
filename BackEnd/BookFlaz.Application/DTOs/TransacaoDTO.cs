using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
	public class TransacaoDTO
	{
		public long Id { get; set; }

		public DateTime DataTransacao { get; set; }

		public int CompradorId { get; set; }

		public int VendedorId { get; set; }
		public long PedidoId { get; set; }
		public decimal ValorFinal { get; set; }
		public int PontosUsados { get; set; }
		public decimal ValorDesconto { get; set; }

		public string EstadoTransacao { get; set; }
	}
}

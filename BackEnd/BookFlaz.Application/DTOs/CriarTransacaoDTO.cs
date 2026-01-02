using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
	public class CriarTransacaoDTO
	{
		[Required]
		public long PedidoId { get; set; }

		[Required]
		public int PontosUsados { get; set; }

	}
}

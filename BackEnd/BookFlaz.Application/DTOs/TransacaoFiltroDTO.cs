using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Domain.Enums;	


namespace BookFlaz.Application.DTOs
{
	public class TransacaoFiltroDTO
	{
		public EstadoTransacao? Estado { get; set; }
		public TipoAnuncio? Tipo { get; set; }
		public DateTime? De { get; set; }
		public DateTime? Ate { get; set; }

		public string? Papel { get; set; }

	}
}

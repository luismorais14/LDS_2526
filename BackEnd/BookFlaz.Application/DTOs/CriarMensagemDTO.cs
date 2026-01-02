using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class CriarMensagemDTO
    {
        public long? ConversaId { get; set; }

        public required long AnuncioId { get; set; }

        public required string Conteudo { get; set; }
    }
}

using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class CriarNotificacaoDTO
    {
        [Required]
        [StringLength(300)]
        [MinLength(1)]
        public string Conteudo { get; set; }

        [Required]
        public TipoNotificacao tipoNotificacao { get; set; }

        [Required]
        [ForeignKey("Client")]
        public long ClientId { get; set; }

    }
}

﻿using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs
{
    public class CriarPedidoTransacaoDTO
    {
        [Required]
        public double ValorProposto { get; set; }

        [Required]
        public long AnuncioId { get; set; }

        public long? ConversaId { get; set; }

        public int? DiasDeAluguel { get; set; }
    }
}

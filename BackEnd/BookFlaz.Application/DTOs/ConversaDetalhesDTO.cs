using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.DTOs;

public class ConversaDetalhesDTO
{
    public List<Mensagem> Mensagens { get; set; } = new();
    public List<PedidoTransacao> Pedidos { get; set; } = new();
}


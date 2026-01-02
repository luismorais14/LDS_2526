using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Interfaces
{
    public interface IChatService
    {
        Task<Mensagem> EnviarMensagem(CriarMensagemDTO mensagem, long userId);
        Task<ConversaDetalhesDTO> ObterMensagensPorConversa(long conversaId);
        Task<List<Conversa>> ObterConversasPorUsuario(long usuarioId);
        Task<Conversa> ObterConversa(long id);
    }
}

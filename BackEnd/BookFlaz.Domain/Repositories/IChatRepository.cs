using BookFlaz.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface IChatRepository
    {
        Task<Conversa?> ObterConversaAsync(long idConversa);
        Task<Conversa?> ObterConversaUsandoDadosAsync(long clientId, long vendedorId, long anuncioId);

        Task<Conversa> CriarConversaAsync(long vendedorId, long compradorId, long anuncioId);

        Task<Mensagem> CriarMensagemAsync(long clienteId, long conversaId, string conteudo);

        Task<bool> ConversaExisteAsync(long conversaId);

        Task<List<Mensagem>> ObterMensagensPorConversaAsync(long conversaId);

        Task<List<Conversa>> ObterConversasPorUsuarioAsync(long usuarioId);
        Task<List<PedidoTransacao>> ObterPedidosNaConversaAsync(long conversaId);
    }
}
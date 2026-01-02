using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookFlaz.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly BooksContext _context;

        public ChatRepository(BooksContext context)
        {
            _context = context;
        }

        public async Task<Conversa?> ObterConversaAsync(long idConversa)
        {
            return await _context.Conversas
                .FirstOrDefaultAsync(c =>c.Id == idConversa);
        }

        public async Task<Conversa?> ObterConversaUsandoDadosAsync(long clientId, long vendedorId, long anuncioId)
        {
            return await _context.Conversas.FirstOrDefaultAsync(c => c.VendedorId == vendedorId && c.CompradorId == clientId && c.AnuncioId == anuncioId);
        }

        public async Task<Conversa> CriarConversaAsync(long vendedorId, long compradorId, long anuncioId)
        {
            var conversa = Conversa.CriarConversa(vendedorId, compradorId, anuncioId);
            _context.Conversas.Add(conversa);
            await _context.SaveChangesAsync();
            return conversa;
        }

        public async Task<Mensagem> CriarMensagemAsync(long clienteId, long conversaId, string conteudo)
        {
            var mensagem = Mensagem.CriarMensagem(clienteId, conversaId, conteudo);
            _context.Mensagens.Add(mensagem);
            await _context.SaveChangesAsync();
            return mensagem;
        }

        public async Task<bool> ConversaExisteAsync(long conversaId)
        {
            return await _context.Conversas
                .AsNoTracking()
                .AnyAsync(c => c.Id == conversaId);
        }

        public async Task<List<Mensagem>> ObterMensagensPorConversaAsync(long conversaId)
        {
            return await _context.Mensagens
                .AsNoTracking()
                .Where(m => m.ConversaId == conversaId)
                .OrderBy(m => m.DataEnvio)
                .ToListAsync();
        }

        public async Task<List<PedidoTransacao>> ObterPedidosNaConversaAsync(long conversaId)
        {
            return await _context.PedidosTransacao
                .AsNoTracking()
                .Where(p => p.ConversaId == conversaId)
                .ToListAsync();
        }

        public async Task<List<Conversa>> ObterConversasPorUsuarioAsync(long usuarioId)
        {
            return await _context.Conversas
                .AsNoTracking()
                .Where(c => c.CompradorId == usuarioId || c.VendedorId == usuarioId)
                .ToListAsync();
        }
    }
}
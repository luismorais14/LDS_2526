using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookFlaz.Infrastructure.Repositories;

public class AvaliacaoRepository : IAvaliacaoRepository
{
    private readonly BooksContext _context;

    public AvaliacaoRepository(BooksContext context)
    {
        _context = context;
    }
    
    public async Task<bool> ExisteAsync(long transacaoId, long clienteId)
    {
        var transacao = await _context.Avaliacoes.FindAsync(transacaoId);
        
        if (transacao == null)
        {
            return false;
        }

        var cliente = (transacao.AvaliadoId == clienteId) || (transacao.AvaliadorId ==  clienteId);

        if (!cliente)
        {
            return false;
        }

        return true;
    }

    public async Task AdicionarAsync(Avaliacao avaliacao)
    {
        await _context.Avaliacoes.AddAsync(avaliacao);
    }

    public async Task<(double Media, int Total)> CalcularReputacaoAsync(long clienteId)
    {
        var query = _context.Avaliacoes.Where(a => a.AvaliadoId == clienteId);

        var total = await query.CountAsync();
        if (total == 0)
        {
            return (Media: 0.0,  Total: 0);
        }

        var media = await query.AverageAsync(a => a.Estrelas);
        
        return (Media: media, Total: total);
    }
}
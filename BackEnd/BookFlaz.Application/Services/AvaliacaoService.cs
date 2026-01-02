using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using Mapster;

namespace BookFlaz.Application.Services;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly IAvaliacaoRepository _avaliacaoRepo;
    private readonly ITransacaoRepository _transacaoRepo;
    private readonly IClienteRepository _clienteRepo;

    public AvaliacaoService(IAvaliacaoRepository avaliacaoRepo, ITransacaoRepository transacaoRepo,  IClienteRepository clienteRepo)
    {
        _avaliacaoRepo = avaliacaoRepo;
        _transacaoRepo = transacaoRepo;
        _clienteRepo = clienteRepo;
    }

    /// <summary>
    /// Cria uma avaliação após uma transação
    /// </summary>
    /// <param name="avaliadorId">O id do avaliador a fazer a avaliação</param>
    /// <param name="avaliadoId">O id do avaliado a receber a avaliação</param>
    /// <param name="transacaoId">O id da transação a ser avaliada</param>
    /// <param name="dto">Dados necessários para a avaliação</param>
    /// <returns>A avaliação</returns>
    /// <exception cref="NotFoundException">Exceção a ser lançada caso não haja nenhuma transação com o id especificado ou caso não exista nenhum comprador ou vendedor com os ids especificados.</exception>
    public async Task<Avaliacao> AvaliarAsync(long avaliadorId, long avaliadoId, long transacaoId, AvaliacaoDTO dto)
    {
        var transacao = await _transacaoRepo.ObterPorIdAsync(transacaoId);

        if (transacao == null)
        {
            throw new NotFoundException("Transação não encontrada.");
        }

        if (transacao.EstadoTransacao != EstadoTransacao.CONCLUIDA)
        {
            throw new BusinessException("Transação não concluida.");
        }

        var parValido = (transacao.CompradorId == avaliadorId && transacao.VendedorId == avaliadoId) ||
                        (transacao.VendedorId == avaliadorId && transacao.CompradorId == avaliadoId);
        
        if (!parValido)
        {
            throw new BusinessException("Os utilizadores não pertencem a esta transação.");
        }

        if (avaliadorId == avaliadoId)
        {
            throw new BusinessException("Avaliador e avaliado não podem ser a mesma pessoa.");
        }

        var jaAvaliado = await _avaliacaoRepo.ExisteAsync(transacaoId, avaliadorId);

        if (jaAvaliado)
        {
            throw new BusinessException("Já existe avaliação deste utilizador para esta transação.");
        }
        
        if (dto.Estrelas is < 1 or > 5)
            throw new BusinessException("Número de estrelas deve estar entre 1 e 5.");

        var avaliacao = new Avaliacao();
        
        avaliacao.AdicionarAvaliado(avaliadoId);
        avaliacao.AdicionarAvaliador(avaliadorId);
        avaliacao.AdicionarEstrelas(dto.Estrelas);
        avaliacao.AdicionarComentario(dto.Comentario);
        avaliacao.AdicionarTransacao(transacaoId);
        
        await _avaliacaoRepo.AdicionarAsync(avaliacao);

        var (novaMedia, totalAvaliacoes) = await _avaliacaoRepo.CalcularReputacaoAsync(avaliadoId);
        
        var clienteAvaliado = await  _clienteRepo.ObterPorIdAsync(avaliadoId);

        if (clienteAvaliado != null)
        {
            clienteAvaliado.AtualizarReputacao(novaMedia, totalAvaliacoes);
            await _clienteRepo.AtualizarAsync(clienteAvaliado);
        }

        return avaliacao;
    }
}
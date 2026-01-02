using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Exceptions;
using BookFlaz.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Services
{
    /// <summary>
    /// Serviço responsável pela gestão do sistema de pontos dos clientes.
    /// Permite consultar os pontos de um utilizador autenticado.
    /// </summary>
    public class PontosService : IPontosService
    {
        private readonly IMovimentoPontosRepository _movRepo;
        private readonly ITransacaoRepository _transRepo;
        private readonly IPedidoTransacaoRepository _pedidoRepo;
        private readonly IAnuncioRepository _anuncioRepo;
        private readonly IClienteRepository _clienteRepo;

        public PontosService(
            IMovimentoPontosRepository movRepo,
            ITransacaoRepository transRepo,
            IPedidoTransacaoRepository pedidoRepo,
            IAnuncioRepository anuncioRepo,
            IClienteRepository clienteRepo)
        {
            _movRepo = movRepo;
            _transRepo = transRepo;
            _pedidoRepo = pedidoRepo;
            _anuncioRepo = anuncioRepo;
            _clienteRepo = clienteRepo;
        }


        /// <summary>
        /// Obtém o saldo de pontos de um cliente.
        /// </summary>
        /// <param name="clienteId">ID do cliente cujo saldo de pontos será consultado.</param>
        /// <returns>Retorna um inteiro representando o número total de pontos do cliente.</returns>
        /// <exception cref="NotFoundException">Lançada quando o cliente não existe.</exception>
        /// <exception cref="ApplicationException">Lançada quando ocorre um erro inesperado ao obter os pontos.</exception>
        public async Task<int> ObterPontosAsync(long clienteId)
        {
            try
            {
                if (!await _clienteRepo.ExisteAsync(clienteId))
                {
                    throw new NotFoundException($"Cliente {clienteId} não existe.");
                }

                return await _clienteRepo.ObterPontosAsync(clienteId);
            }
            catch (NotFoundException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao obter pontos do cliente.", ex);
            }
        }

        /// <summary>
        /// Obtém o histórico de movimentações de pontos de um cliente.
        /// </summary>
        /// <param name="clienteId">ID do cliente para o qual se pretende obter o histórico de pontos.</param>
        /// <returns>Lista com o histórico de movimentações de pontos (ganhos e usos), ordenada por data decrescente.</returns>
        /// <exception cref="NotFoundException">Lançada se o cliente não existir ou não possuir movimentações de pontos.</exception>
        public async Task<List<PontosMovimentadosDTO>> ObterHistoricoAsync(long clienteId)
        {
            try
            {
                await VerificarClienteExistente(clienteId);

                var resultado = await ObterHistorico(clienteId);

                return resultado;
            }
            catch (NotFoundException)
            {
                throw;
            }
        }

        /// <summary>
        /// Adiciona pontos ao saldo de um cliente com base numa transação concluída.
        /// </summary>
        /// <remarks>
        /// Este método é utilizado quando o cliente realiza uma ação que gera pontos — por exemplo,
        /// a conclusão bem-sucedida de uma compra ou venda.  
        /// </remarks>
        /// <param name="clienteId">Identificador único do cliente que receberá os pontos.</param>
        /// <param name="pontos">Quantidade de pontos a adicionar.</param>
        /// <param name="transacaoId">Identificador da transação associada ao ganho de pontos.</param>
        /// <returns>Uma tarefa assíncrona que representa a operação de atualização do saldo de pontos.</returns>
        /// <exception cref="NotFoundException">Lançada quando o cliente ou a transação não são encontrados na base de dados.</exception>
        /// <exception cref="ValidationException"> Lançada quando ocorre uma violação de regra de domínio ao adicionar os pontos.</exception>
        public async Task AdicionarPontos(long clienteId, int pontos, long transacaoId)
        {
            try
            {
                var cliente = await _clienteRepo.ObterPorIdAsync(clienteId);

                if (cliente == null)
                {
                    throw new NotFoundException($"Cliente {clienteId} não existe.");
                }

                var transacao = await _transRepo.ObterPorIdAsync(transacaoId);

                if (transacao == null)
                {
                    throw new NotFoundException($"Transação {transacaoId} não existe.");
                }

                cliente.AdicionarPontos(pontos);

                await _clienteRepo.AtualizarAsync(cliente);

                var movimento = MovimentoPontos.AdicionarMovimento(clienteId, transacaoId, TipoMovimento.GANHO, pontos);

                await _movRepo.AdicionarMovimento(movimento);
            }
            catch (DomainException ex)
            {
                throw new ValidationException(ex.Message);
            }
        }

        /// <summary>
        /// Remove pontos do saldo de um cliente com base numa transação em que os pontos são utilizados.
        /// </summary>
        /// <remarks>
        /// Este método é chamado quando o cliente utiliza pontos, por exemplo,
        /// para obter desconto numa transação.  
        /// </remarks>
        /// <param name="clienteId">Identificador único do cliente cujos pontos serão removidos.</param>
        /// <param name="pontos">Quantidade de pontos a deduzir.</param>
        /// <param name="transacaoId">Identificador da transação associada ao gasto de pontos.</param>
        /// <returns>Uma tarefa assíncrona que representa a operação de dedução dos pontos.</returns>
        /// <exception cref="NotFoundException">Lançada quando o cliente ou a transação não são encontrados na base de dados.</exception>
        /// <exception cref="ValidationException">Lançada quando ocorre uma violação de regra de domínio ao remover os pontos</exception>
        public async Task RemoverPontos(long clienteId, int pontos, long transacaoId)
        {
            try
            {
                var cliente = await _clienteRepo.ObterPorIdAsync(clienteId);

                if (cliente == null)
                {
                    throw new NotFoundException($"Cliente {clienteId} não existe.");
                }

                var transacao = await _transRepo.ObterPorIdAsync(transacaoId);

                if (transacao == null)
                {
                    throw new NotFoundException($"Transação {transacaoId} não existe.");
                }

                cliente.RemoverPontos(pontos);

                await _clienteRepo.AtualizarAsync(cliente);

                var movimento = MovimentoPontos.AdicionarMovimento(clienteId, transacaoId, TipoMovimento.GASTO, pontos);

                await _movRepo.AdicionarMovimento(movimento);
            }
            catch (DomainException ex)
            {
                throw new ValidationException(ex.Message);
            }
        }

        /// <summary>
        /// Verifica se um cliente existe no sistema.
        /// </summary>
        /// <param name="clienteId">ID do cliente.</param>
        /// <exception cref="NotFoundException">Lançada se o cliente não existir na base de dados.</exception>
        private async Task VerificarClienteExistente(long clienteId)
        {
            if (!await _clienteRepo.ExisteAsync(clienteId))
            {
                throw new NotFoundException($"Cliente {clienteId} não existe.");
            }
        }

        /// <summary>
        /// Recupera e monta o histórico detalhado de pontos do cliente.
        /// </summary>
        /// <remarks>
        /// Este método:
        /// <list type="bullet">
        /// <item>Obtém todos os movimentos de pontos do cliente</item>
        /// <item>Ordena os movimentos por data (mais recentes primeiro)</item>
        /// <item>Para cada movimento, verifica a transação associada</item>
        /// <item>Ignora transações sem uso de pontos</item>
        /// <item>Obtém dados do pedido e do anúncio quando relevantes</item>
        /// </list>
        /// 
        /// Apenas movimentos associados a transações com pontos utilizados são retornados.
        /// </remarks>
        /// <param name="clienteId">ID do cliente.</param>
        /// <returns>Lista de objetos <see cref="PontosMovimentadosDTO"/> com detalhes da transação e anúncio.</returns>
        /// <exception cref="NotFoundException">Lançada caso o cliente não tenha movimentos registados.</exception>

        private async Task<List<PontosMovimentadosDTO>> ObterHistorico(long clienteId)
        {
            var movimentos = await _movRepo.ObterMovimentosPorClienteAsync(clienteId);

            if (!movimentos.Any())
            {
                throw new NotFoundException("O cliente ainda não tem nenhuma atividade associada aos pontos");
            }

            var movimentosOrdenados = movimentos
                .OrderByDescending(m => m.DataMovimento)
                .ToList();

            var resultado = new List<PontosMovimentadosDTO>();

            foreach (var mov in movimentosOrdenados)
            {
                long? anuncioId = null;
                long? conversaId = null;
                string? imagem = null;
                string? titulo = null;
                decimal? preco = null;

                if (mov.TransacaoId.HasValue)
                {
                    var transacao = await _transRepo.ObterPorIdAsync(mov.TransacaoId.Value);

                    if (transacao != null)
                    {
                        var pedido = await _pedidoRepo.ObterPorIdAsync(transacao.PedidoId);

                        if (pedido != null)
                        {
                            anuncioId = pedido.AnuncioId;
                            conversaId = pedido.ConversaId;

                            var anuncio = await _anuncioRepo.ObterPorIdAsync(anuncioId.Value);

                            if (anuncio != null)
                            {
                                imagem = anuncio.Imagens?.Split(';').FirstOrDefault();
                                titulo = anuncio.Livro?.Titulo;
                                preco = anuncio.Preco;
                            }
                        }   
                    }
                }

                resultado.Add(new PontosMovimentadosDTO
                {
                    Quantidade = mov.Quantidade,
                    Data = mov.DataMovimento,
                    TipoMovimento = mov.TipoMovimento,
                    AnuncioId = anuncioId,
                    ConversaId = conversaId,
                    ImagemAnuncio = imagem,
                    TituloAnuncio = titulo,
                    Preco = preco
                });
            }

            return resultado;
        }

    }
}
﻿using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Services
{
    /// <summary>
    /// Serviço responsável por gerir o ciclo de vida dos pedidos de transação 
    /// entre utilizadores, garantindo as regras de negócio e integridade dos dados.
    /// 
    /// Este serviço atua como intermediário entre os controladores (camada Web/API)
    /// e o domínio, realizando validações,  e tratamento de exceções
    /// específicas da aplicação.
    /// </summary>
    public class PedidoTransacaoService : IPedidoTransacaoService
    {
        private readonly IPedidoTransacaoRepository _pedidoRepo;
        private readonly IConversaRepository _conversaRepo;
        private readonly IAnuncioRepository _anuncioRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly INotificacaoService _notificacaoService;
        private readonly IFavoritoRepository _favoritoRepository;

        public PedidoTransacaoService(
            IPedidoTransacaoRepository pedidoRepo,
            IConversaRepository conversaRepo,
            IAnuncioRepository anuncioRepo,
            IClienteRepository clienteRepo,
            INotificacaoService notificacaoService,
            IFavoritoRepository favoritoRepository)
        {
            _pedidoRepo = pedidoRepo;
            _conversaRepo = conversaRepo;
            _anuncioRepo = anuncioRepo;
            _clienteRepo = clienteRepo;
            _notificacaoService = notificacaoService;
            _favoritoRepository = favoritoRepository;
        }

        /// <summary>
        /// Cria um novo pedido de transação com base nos dados fornecidos no DTO.
        /// 
        /// O método valida:
        /// - Se o anúncio e o utilizador existem;
        /// - Se o valor proposto é válido (maior que zero, quando aplicável);
        /// - Se o vendedor não tenta iniciar uma conversa sem proposta prévia;
        /// - Se existe um pedido pendente anterior entre as partes (cancelando-o automaticamente).
        /// 
        /// Caso todas as condições sejam satisfeitas, o pedido é criado e registado no repositório.
        /// </summary>
        /// <param name="dto">Dados necessários para criar o pedido.</param>
        /// <exception cref="ValidationException">Quando há erros de validação dos dados.</exception>
        /// <exception cref="UnauthorizedActionException">Quando a ação não é permitida ao utilizador atual.</exception>
        /// <exception cref="BusinessException">Quando alguma regra de negócio é violada.</exception>
        /// <exception cref="ApplicationException">Quando ocorre um erro inesperado na criação do pedido.</exception>
        /// <exception cref="NotFoundException">Quando algo não é encontrado.</exception>
        public async Task<(long PedidoId, long ConversaId)> CriarPedidoAsync(CriarPedidoTransacaoDTO dto, long idCliente)
        {
            try
            {
                var anuncio = await ValidarDadosIniciaisAsync(dto.ValorProposto, dto.AnuncioId, idCliente, dto.DiasDeAluguel);

                await ValidarInteracaoAsync(anuncio, idCliente, dto.ConversaId);

                var conversa = await ObterOuCriarConversaAsync(dto.ConversaId, anuncio, idCliente);

                await VerificarUltimoPedido(conversa.Id);

                await CancelarPedidoPendenteAnteriorAsync(conversa.CompradorId, anuncio);

                var remetenteId = idCliente;

                var destinatarioId = (idCliente == anuncio.VendedorId)
                    ? conversa.CompradorId
                    : anuncio.VendedorId;

                var pedido = PedidoTransacao.CriarPedido(
                    dto.ValorProposto,
                    anuncio.TipoAnuncio,
                    anuncio.Id,
                    conversa.CompradorId,
                    anuncio.VendedorId,
                    destinatarioId,
                    remetenteId,
                    conversa.Id,
                    dto.DiasDeAluguel
                );

                await _pedidoRepo.AdicionarAsync(pedido);

                await _notificacaoService.CriarNotificacaoAsync("Recebeu um pedido de transação no seu anuncio " + anuncio.Livro.Titulo, TipoNotificacao.TRANSACAO, destinatarioId);

                return (pedido.Id, conversa.Id);
            }
            catch (BusinessException ex)
            {
                throw;
            }
            catch (ValidationException ex)
            {
                throw;
            }
            catch (UnauthorizedActionException ex)
            {
                throw;
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ocorreu um erro inesperado ao criar o pedido. Tente novamente mais tarde.");
            }
        }

        /// <summary>
        /// Aceita um pedido de transação pendente, alterando o estado do pedido para <see cref="EstadoPedido.ACEITE"/>.
        /// 
        /// Apenas o destinatário do pedido pode aceitar a proposta.
        /// Caso o pedido já tenha sido processado ou pertença a outro utilizador, é lançada uma exceção.
        /// </summary>
        /// <param name="pedidoId">Identificador do pedido de transação.</param>
        /// <param name="utilizadorId">Identificador do utilizador que tenta aceitar o pedido.</param>
        /// <exception cref="NotFoundException">Se o pedido não for encontrado.</exception>
        /// <exception cref="BusinessException">Se o pedido já tiver sido processado ou o utilizador não for o destinatário.</exception>
        /// <exception cref="ApplicationException">Se ocorrer um erro inesperado.</exception>
        public async Task AceitarPedidoAsync(long pedidoId, long utilizadorId)
        {
            try
            {
                var pedido = await _pedidoRepo.ObterPorIdAsync(pedidoId);

                if (pedido == null)
                {
                    throw new NotFoundException("Pedido não encontrado.");
                }
                ;

                pedido.Aceitar(utilizadorId);

                await _pedidoRepo.AtualizarAsync(pedido);
                List<Favorito>? favoritosAnuncio = await _favoritoRepository.ObterPorAnuncioAsync(pedido.AnuncioId);

                foreach (Favorito fav in favoritosAnuncio)
                {
                    await _notificacaoService.CriarNotificacaoAsync("Seu Favorito (anúncio: " + pedido.AnuncioId + ") não se encontra mais disponível!", TipoNotificacao.FAVORITO, fav.ClienteId);
                }
            }
            catch (BusinessException ex)
            {
                throw;
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao aceitar o pedido.");
            }
        }

        /// <summary>
        /// Rejeita um pedido de transação pendente, alterando o estado para <see cref="EstadoPedido.REJEITADO"/>.
        /// 
        /// Apenas o destinatário do pedido pode rejeitar a proposta.
        /// Caso o pedido já tenha sido processado ou o utilizador não seja o destinatário, 
        /// é lançada uma exceção de negócio.
        /// </summary>
        /// <param name="pedidoId">Identificador do pedido de transação.</param>
        /// <param name="utilizadorId">Identificador do utilizador que tenta rejeitar o pedido.</param>
        /// <exception cref="NotFoundException">Se o pedido não for encontrado.</exception>
        /// <exception cref="BusinessException"> Se o pedido já tiver sido processado ou o utilizador não for o destinatário.
        /// </exception>
        /// <exception cref="ApplicationException">Se ocorrer um erro inesperado.</exception>
        public async Task RejeitarPedidoAsync(long pedidoId, long utilizadorId)
        {
            try
            {
                var pedido = await _pedidoRepo.ObterPorIdAsync(pedidoId);

                if (pedido == null)
                {
                    throw new NotFoundException("Pedido não encontrado.");
                }
                ;

                pedido.Rejeitar(utilizadorId);

                await _pedidoRepo.AtualizarAsync(pedido);
            }
            catch (BusinessException ex)
            {
                throw;
            }
            catch (NotFoundException ex)
            {
                throw;
            }
            catch (InvalidOperationException ex)
            {
                throw new BusinessException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao rejeitar o pedido.");
            }
        }

        /// <summary>
        /// Valida os dados iniciais necessários para a criação de um pedido de transação.
        /// 
        /// Este método assegura que:
        /// - O anúncio associado ao pedido existe;
        /// - O utilizador que realiza a ação está registado no sistema;
        /// - O número de dias de aluguer é válido (quando aplicável);
        /// - O valor proposto é coerente com o tipo de anúncio.
        /// 
        /// Caso alguma destas condições não seja satisfeita, uma exceção adequada é lançada.
        /// </summary>
        /// <param name="valorProposto">Valor proposto para a transação.</param>
        /// <param name="anuncioId">Identificador do anúncio associado ao pedido.</param>
        /// <param name="utilizadorId">Identificador do utilizador que realiza a operação.</param>
        /// <param name="diasDeAluguel">
        /// Número de dias de aluguer (obrigatório apenas quando o tipo de anúncio é <see cref="TipoAnuncio.ALUGUER"/>).
        /// </param>
        /// <returns>O objeto <see cref="Anuncio"/> validado, pronto para ser utilizado na criação do pedido.</returns>
        /// <exception cref="NotFoundException"> Lançada quando o anúncio ou o utilizador especificado não existem. </exception>
        /// <exception cref="ValidationException"> Lançada quando o valor proposto é inválido ou quando o número de dias de aluguer não é informado em anúncios do tipo aluguer. </exception>
        private async Task<Anuncio> ValidarDadosIniciaisAsync(double valorProposto, long anuncioId, long utilizadorId, int? diasDeAluguel)
        {
            var anuncio = await _anuncioRepo.ObterPorIdAsync(anuncioId);

            if (anuncio == null)
            {
                throw new NotFoundException("O anúncio especificado não existe.");
            }

            if (anuncio.TipoAnuncio == TipoAnuncio.ALUGUER && (diasDeAluguel == null || diasDeAluguel <= 0))
            {
                throw new ValidationException("Deves indicar o número de dias para o aluguel.");
            }

            var clienteExiste = await _clienteRepo.ExisteAsync(utilizadorId);

            if (!clienteExiste)
            {
                throw new NotFoundException("O utilizador especificado não existe.");
            }

            if (valorProposto <= 0 && anuncio.TipoAnuncio != TipoAnuncio.DOACAO)
            {
                throw new ValidationException("O valor proposto deve ser maior que zero.");
            }

            if (valorProposto > 10000 && anuncio.TipoAnuncio != TipoAnuncio.DOACAO)
            {
                throw new ValidationException("O valor proposto deve ser menor que 10000.");
            }

            if (diasDeAluguel is not null)
            {
                if (diasDeAluguel <= 0)
                {
                    throw new ValidationException("Os dias de alguel tem de ser superior a 0.");
                }
            }

            return anuncio;
        }

        /// <summary>
        /// Valida se o utilizador tem permissão para interagir numa determinada conversa relacionada a um anúncio.
        /// 
        /// Esta verificação garante que o vendedor não pode iniciar uma proposta de transação
        /// sem que o comprador tenha iniciado previamente a conversa.  
        /// Caso essa condição não seja cumprida, é lançada uma exceção de autorização.
        /// </summary>
        /// <param name="anuncio">O anúncio associado à conversa.</param>
        /// <param name="utilizadorId">Identificador do utilizador que tenta criar o pedido.</param>
        /// <param name="conversa">A conversa na qual o pedido está a ser criado.</param>
        /// <exception cref="UnauthorizedActionException"> Lançada quando o vendedor tenta enviar uma proposta sem que o comprador tenha iniciado a conversa.</exception>
        private async Task ValidarInteracaoAsync(Anuncio anuncio, long utilizadorId, long? conversaId)
        {
            if (anuncio.VendedorId == utilizadorId)
            {
                if (conversaId is null)
                {
                    throw new UnauthorizedActionException("O vendedor não pode enviar uma proposta sem o comprador ter iniciado a conversa.");
                }
            }
        }

        /// <summary>
        /// Obtém uma conversa existente ou cria uma nova, consoante o contexto do pedido de transação.
        /// 
        /// Caso um conversaId válido seja fornecido, o método tenta recuperar a conversa do repositório.
        /// Caso não exista um conversaId, é criada automaticamente uma nova conversa
        /// entre o utilizador atual e o vendedor do anúncio.
        /// </summary>
        /// <param name="conversaId">Identificador opcional da conversa existente (pode ser nulo).</param>
        /// <param name="anuncio">Anúncio associado ao pedido de transação.</param>
        /// <param name="utilizadorId">Identificador do utilizador que está a criar ou a usar a conversa.</param>
        /// <returns>Um objeto <see cref="Conversa"/> válido, existente ou recém-criado.</returns>
        /// <exception cref="NotFoundException">Se a conversa fornecida não existir.</exception>
        /// <exception cref="UnauthorizedActionException"> Se o utilizador não fizer parte da conversa existente (nem comprador nem vendedor). </exception>
        private async Task<Conversa> ObterOuCriarConversaAsync(long? conversaId, Anuncio anuncio, long utilizadorId)
        {
            if (conversaId is long id)
            {
                var conversa = await _conversaRepo.ObterPorIdAsync(id);

                if (conversa == null)
                {
                    throw new NotFoundException("A conversa especificada não existe.");
                }

                if (conversa.CompradorId != utilizadorId && conversa.VendedorId != utilizadorId)
                {
                    throw new UnauthorizedActionException("Não podes interagir numa conversa onde não participas.");
                }

                return conversa;
            }

            var conversaExistente = await _conversaRepo.ObterEntreAsync(
                compradorId: utilizadorId,
                vendedorId: anuncio.VendedorId,
                anuncioId: anuncio.Id
            );

            if (conversaExistente != null)
            {
                return conversaExistente;
            }

            var nova = Conversa.CriarConversa(anuncio.VendedorId, utilizadorId, anuncio.Id);

            await _conversaRepo.AdicionarAsync(nova);

            return nova;
        }

        /// <summary>
        /// Cancela automaticamente um pedido pendente existente entre o utilizador e o vendedor
        /// para o mesmo anúncio, antes de criar um novo pedido de transação.
        /// 
        /// Esta operação garante que apenas um pedido ativo (pendente) pode existir entre as mesmas partes
        /// e para o mesmo anúncio, evitando duplicações e inconsistências. Caso seja encontrado um 
        /// pedido pendente, o seu estado é alterado para cancelado e a atualização é persistida
        /// no repositório.
        /// </summary>
        /// <param name="utilizadorId">Identificador do utilizador que está a criar o novo pedido.</param>
        /// <param name="anuncio">Anúncio associado ao pedido de transação.</param>
        /// <exception cref="ApplicationException">Lançada quando ocorre um erro inesperado ao tentar cancelar o pedido pendente anterior. </exception>
        private async Task CancelarPedidoPendenteAnteriorAsync(long utilizadorId, Anuncio anuncio)
        {
            try
            {
                var pendente = await _pedidoRepo.ObterPendenteEntreAsync(utilizadorId, anuncio.VendedorId, anuncio.Id);

                if (pendente != null)
                {
                    pendente.Cancelar();

                    await _pedidoRepo.AtualizarAsync(pendente);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao cancelar pedido pendente anterior.");
            }
        }

        /// <summary>
        /// Verifica o estado do último pedido associado a uma conversa específica.
        /// 
        /// Este método garante que, caso já exista um pedido de transação com estado 
        /// <see cref="EstadoPedido.ACEITE"/> entre as duas partes para o mesmo anúncio,
        /// não seja possível criar um novo pedido na mesma conversa.
        /// </summary>
        /// <param name="idConversa">Identificador da conversa onde os pedidos de transação foram trocados. </param>
        /// <exception cref="BusinessException">Lançada quando o último pedido na conversa já foi aceite. </exception>
        private async Task VerificarUltimoPedido(long idConversa)
        {
            var ultimoPedido = await _pedidoRepo.ObterUltimoPedidoDaConversaAsync(idConversa);

            if (ultimoPedido != null && ultimoPedido.EstadoPedido == EstadoPedido.ACEITE)
            {
                throw new BusinessException("Já existe uma transação aceite nesta conversa para este anúncio. Não é possível criar um novo pedido.");
            }
        }

        public async Task<PedidoTransacao?> ObterPorIdAsync(long id, long userId)
        {
            var pt = await _pedidoRepo.ObterPorIdAsync(id);

            if (pt == null)
            {
                return null;
            }

            if (pt.CompradorId == userId)
            {
                return pt;
            }

            return null;
        }
    }
}

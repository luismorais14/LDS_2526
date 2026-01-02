using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using BookFlaz.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;
        private readonly IAnuncioRepository _anuncioRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly INotificacaoService _notificacaoServ;

        public ChatService(IChatRepository repo, IAnuncioRepository anuncioRepository, IClienteRepository clienteRepository, INotificacaoService notificacaoServ)
        {
            _repo = repo;
            _anuncioRepository = anuncioRepository;
            _clienteRepository = clienteRepository;
            _notificacaoServ = notificacaoServ;
        }

        public async Task<Mensagem> EnviarMensagem(CriarMensagemDTO mensagem, long userId)
        {
            try
            {
                if (mensagem is null) throw new ValidationException("Pedido inválido.");
                if (string.IsNullOrWhiteSpace(mensagem.Conteudo))
                    throw new ValidationException("Conteúdo da mensagem é obrigatório.");

                var anuncio = await _anuncioRepository.ObterPorIdAsync(mensagem.AnuncioId)
                ?? throw new NotFoundException("Anúncio não encontrado.");

                var clienteExiste = await _clienteRepository.ExisteAsync(userId);
                if (!clienteExiste)
                    throw new NotFoundException("Cliente não encontrado.");

                Conversa? conversa = null;

                if (mensagem.ConversaId.HasValue)
                {
                    conversa = await _repo.ObterConversaAsync(mensagem.ConversaId.Value)
                        ?? throw new NotFoundException("Conversa não encontrada.");

                    if (conversa.AnuncioId != anuncio.Id)
                        throw new ValidationException("Conversa não pertence ao anúncio indicado.");

                    var ehParte = (conversa.CompradorId == userId) || (conversa.VendedorId == userId);
                    if (!ehParte)
                        throw new UnauthorizedActionException("Utilizador não pertence à conversa.");
                }
                else
                {
                    conversa = await _repo.ObterConversaUsandoDadosAsync(userId, anuncio.VendedorId, anuncio.Id);

                    if (conversa == null)
                    {
                        if (anuncio.VendedorId == userId)
                            throw new UnauthorizedActionException("Anunciante tentou iniciar conversa consigo mesmo.");

                        conversa = await _repo.CriarConversaAsync(anuncio.VendedorId, userId, anuncio.Id);
                    }
                }

                var novaMensagem = await _repo.CriarMensagemAsync(userId, conversa.Id, mensagem.Conteudo);

                await _notificacaoServ.CriarNotificacaoAsync("Recebes-te Mensagem do utilizador " + userId, TipoNotificacao.CHAT, anuncio.VendedorId);

                return novaMensagem;
            }
            catch (NotFoundException) 
            { 
                throw; 
            }
            catch (UnauthorizedActionException) 
            {
                throw;
            }
            catch (ValidationException) 
            {
                throw;
            }
            catch (ArgumentException ex) 
            {
                throw new ValidationException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao enviar a mensagem.", ex);
            }

        }

        public async Task<Conversa> ObterConversa(long id)
        {
            try
            {
                var conversa = await _repo.ObterConversaAsync(id);
                if (conversa == null)
                    throw new NotFoundException("Conversa não encontrada.");
                return conversa;
            }
            catch (NotFoundException) 
            { 
                throw; 
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao obter a conversa.", ex);
            }
        }

        public async Task<ConversaDetalhesDTO> ObterMensagensPorConversa(long conversaId)
        {
            try
            {
                if (!await _repo.ConversaExisteAsync(conversaId))
                    throw new NotFoundException("Conversa não encontrada.");

                var mensagens = await _repo.ObterMensagensPorConversaAsync(conversaId);
                var pedidos = await _repo.ObterPedidosNaConversaAsync(conversaId);

                return new ConversaDetalhesDTO
                {
                    Mensagens = mensagens,
                    Pedidos = pedidos
                };
            }
            catch (NotFoundException) 
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao obter mensagens da conversa.", ex);
            }
        }

        public async Task<List<Conversa>> ObterConversasPorUsuario(long usuarioId)
        {
            try
            {
                if (!await _clienteRepository.ExisteAsync(usuarioId))
                    throw new NotFoundException("Usuário não encontrado.");

                return await _repo.ObterConversasPorUsuarioAsync(usuarioId);
            }
            catch (NotFoundException) 
            { 
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao obter conversas do utilizador.", ex);
            }
        }
    }
}

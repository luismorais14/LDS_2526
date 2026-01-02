using BookFlaz.Application.DTOs;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Services
{
    public class NotificacaoService : INotificacaoService
    {
        private readonly INotificacaoRepository _repository;
        public NotificacaoService(INotificacaoRepository notificacao) 
        {
            _repository = notificacao;
        }

        public async Task<Notificacao> CriarNotificacaoAsync(string conteudo, TipoNotificacao tipoNotificacao, long clientId)
        {
            try
            {
                if (clientId <= 0)
                    throw new ArgumentOutOfRangeException(nameof(clientId), "ID do cliente inválido.");
                if (string.IsNullOrWhiteSpace(conteudo))
                    throw new ArgumentException("Conteúdo da notificação é obrigatório.", nameof(conteudo));

                var notificacaoCriada = Notificacao.CriarNotificacao(conteudo, tipoNotificacao, clientId);
                await _repository.CriarNotificacao(notificacaoCriada);
                return notificacaoCriada;
            }
            catch (ArgumentException) 
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro inesperado ao criar notificação.", ex);
            }
        }

        public async Task<List<Notificacao>> ObterNotificacoesAsync(long id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentOutOfRangeException(nameof(id), "ID do cliente inválido.");

                var lista = await _repository.ObterNotificacoes(id);

                return lista.OrderByDescending(n => n.DataEnvio).ToList();
            }
            catch (ArgumentOutOfRangeException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao obter notificações.", ex);
            }
        }
    }
}

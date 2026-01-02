using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Application.Interfaces
{
    public interface INotificacaoService
    {
        Task<Notificacao> CriarNotificacaoAsync(string conteudo, TipoNotificacao tipoNotificacao, long clientId);
        Task<List<Notificacao>> ObterNotificacoesAsync(long id);
    }
}

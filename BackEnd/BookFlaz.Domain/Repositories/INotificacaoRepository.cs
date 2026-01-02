using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Repositories
{
    public interface INotificacaoRepository
    {
        Task CriarNotificacao(Notificacao notificacao);
        Task<List<Notificacao>> ObterNotificacoes(long id);
    }
}

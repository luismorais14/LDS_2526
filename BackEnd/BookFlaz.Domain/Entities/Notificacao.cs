using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Notificacao
    {
        [Required]
        public long Id { get; private set; }

        [Required]
        [StringLength(300)]
        [MinLength(1)]
        public string Conteudo { get; private set; }

        [Required]
        public DateTime DataEnvio { get; private set; } = DateTime.Now;

        [Required]
        public TipoNotificacao TipoNotificacao { get; private set; }

        [Required]
        [ForeignKey("Client")]
        public long ClientId { get; private set; }

        public static Notificacao CriarNotificacao(string conteudo, TipoNotificacao tipoNotificacao, long clientId)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
                throw new ArgumentException("Não é possível criar Notificacao. Conteúdo da notificação é obrigatório.", nameof(conteudo));

            if (conteudo.Length > 300)
                throw new ArgumentException("Não é possível criar Notificacao. Conteúdo da notificação não pode exceder 300 caracteres.", nameof(conteudo));

            if (clientId <= 0)
                throw new ArgumentException("Não é possível criar Notificacao. ClientId inválido.", nameof(clientId));

            var notificacao = new Notificacao
            {
                Conteudo = conteudo,
                TipoNotificacao = tipoNotificacao,
                ClientId = clientId
            };

            return notificacao;
        }
    }
}

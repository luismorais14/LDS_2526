using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Mensagem
    {
        public int Id { get; private set; }

        [Required]
        [MaxLength(1000)]
        [MinLength(1)]
        public string Conteudo { get; private set; }

        [Required]
        public DateTime DataEnvio { get; private set; } = DateTime.Now;

        [Required]
        public long ClienteId { get; private set; }

        [Required]
        public long ConversaId { get; private set; }

        internal Mensagem(string conteudo, long clienteId, long conversaId)
        {
            Conteudo = conteudo;
            ClienteId = clienteId;
            ConversaId = conversaId;
        }

        public static Mensagem CriarMensagem(long clienteId, long conversaId, string conteudo)
        {
            if (clienteId <= 0) throw new ArgumentOutOfRangeException(nameof(clienteId), "ClienteId inválido.");
            if (conversaId <= 0) throw new ArgumentOutOfRangeException(nameof(conversaId), "ConversaId inválido.");

            var texto = conteudo?.Trim();
            if (string.IsNullOrEmpty(texto))
                throw new ArgumentException("Conteúdo da mensagem não pode ser vazio.", nameof(conteudo));
            if (texto.Length > 1000)
                throw new ArgumentException("Conteúdo da mensagem não pode exceder 1000 caracteres.", nameof(conteudo));

            return new Mensagem(texto, clienteId, conversaId);
        }
    }
}

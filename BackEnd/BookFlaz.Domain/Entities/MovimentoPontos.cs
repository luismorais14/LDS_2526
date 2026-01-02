using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class MovimentoPontos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; private set; }

        public int Quantidade { get; private set; }

        [Required]
        public DateTime DataMovimento { get; private set; }

        [Required]
        public TipoMovimento TipoMovimento { get; private set; }

        [Required]
        [ForeignKey("Cliente")]
        public long ClienteId { get; private set; }
        public virtual Cliente Cliente { get; private set; }

        [ForeignKey("Transacao")]
        public long? TransacaoId { get; private set; }
        public virtual Transacao? Transacao { get; private set; }

        private MovimentoPontos() {}

        internal MovimentoPontos(long id, long clienteId, long? transacaoId, DateTime dataMovimento, TipoMovimento tipoMovimento, int quantidade)
        {
            Id = id;
            ClienteId = clienteId;
            TransacaoId = transacaoId;
            DataMovimento = dataMovimento;
            TipoMovimento = tipoMovimento;
            Quantidade = quantidade;
        }

        public static MovimentoPontos AdicionarMovimento(long clienteId, long transacaoId, TipoMovimento tipoMovimento, int quantidade)
        {
            var Movimento = new MovimentoPontos
            {
                Quantidade = quantidade,
                DataMovimento = DateTime.UtcNow,
                TipoMovimento = tipoMovimento,
                ClienteId = clienteId,
                TransacaoId = transacaoId
            };

            return Movimento;
        }
    }
}

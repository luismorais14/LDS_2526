using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Devolucao
    {
        public long Id { get; private set; }

        [Required]
		public long clienteId { get; private set; }

		[Required]
        public DateTime DataRegisto { get; private set; }

        public DateTime? DataRececaoVendedor { get; private set; }

        [Required]
        public bool Confirmada { get; private set; }

        [Required]
        [ForeignKey("Transacao")]
        public long TransacaoId { get; private set; }

        

        private Devolucao() { }

        public static Devolucao CriarDevolucao(long idTransacao, long clienteId)
        {
            var devolucao = new Devolucao
            {
                DataRegisto = DateTime.UtcNow,
                DataRececaoVendedor = null,
                Confirmada = false,
                TransacaoId = idTransacao,
                clienteId = clienteId
            };

            return devolucao;
        }

        public void Confirmar(long vendedorId, DateTime dataRececao)
        {
            this.DataRececaoVendedor = dataRececao;
            this.Confirmada = true;
        }
    }
}

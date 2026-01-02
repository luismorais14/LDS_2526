using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class Avaliacao
    {
        public long Id { get; private set; }

        [Required]
        [Range(1, 5, ErrorMessage = "O número de estrelas deve estar entre 1 e 5.")]
        public int Estrelas { get; private set; }

        [Required]
        [StringLength(300, ErrorMessage = "O comentário não pode exceder 1000 caracteres.")]
        [MinLength(1, ErrorMessage = "O comentário deve ter pelo menos 1 caractere.")]
        public string Comentario { get; private set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Data { get; private set; } = DateTime.Now;


        public long TransacaoId { get; private set; }
        public Transacao Transacao { get; private set; } = null!;

        public long AvaliadoId { get; private set; }
        public Cliente Avaliado { get; private set; } = null!;

        public long AvaliadorId { get; private set; }
        public Cliente Avaliador { get; private set; } = null!;

        public void AdicionarEstrelas(int estrelas)
        {
            this.Estrelas = estrelas;
        }

        public void AdicionarComentario(string comentario)
        {
            if (comentario == null)
            {
                this.Comentario = string.Empty;
            }
            else
            {
                this.Comentario = comentario;
            }
        }

        public void AdicionarTransacao(long transacaoId)
        {
            if (transacaoId < 1)
            {
                this.TransacaoId = 0;
            }
            else
            {
                this.TransacaoId = transacaoId;
            }
        }

        public void AdicionarAvaliador(long avaliadorId)
        {
            if (avaliadorId < 1)
            {
                this.AvaliadorId = 0;
            }
            else
            {
                this.AvaliadorId = avaliadorId;
            }
        }

        public void AdicionarAvaliado(long avaliadoId)
        {
            if (avaliadoId < 1)
            {
                this.AvaliadoId = 0;
            }
            else
            {
                this.AvaliadoId = avaliadoId;
            }
        }
    }
}
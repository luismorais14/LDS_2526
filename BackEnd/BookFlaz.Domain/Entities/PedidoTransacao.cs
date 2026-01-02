﻿using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Domain.Entities
{
    public class PedidoTransacao
    {
        public long Id { get; private set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor proposto deve ser maior que zero.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "O valor deve ter no máximo duas casas decimais.")]
        public double ValorProposto { get; private set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DataCriacao { get; private set; } = DateTime.Now;

        [Required]
        public TipoAnuncio TipoAnuncio { get; private set; }

        [Required]
        public EstadoPedido EstadoPedido { get; private set; }

        [Required]
        [ForeignKey("Cliente")]
        public long RemetenteId { get; private set; }

        [Required]
        [ForeignKey("Cliente")]
        public long DestinatarioId { get; private set; }

        [Required]
        [ForeignKey("Anuncio")]
        public long AnuncioId { get; private set; }

        [Required]
        [ForeignKey("Cliente")]
        public long CompradorId { get; private set; }

        [Required]
        [ForeignKey("Cliente")]
        public long VendedorId { get; private set; }

        [Required]
        [ForeignKey("Conversa")]
        public long ConversaId { get; private set; }
        public int? DiasDeAluguel { get; private set; }

        private PedidoTransacao() { }

        /// <summary>
        /// Construtor interno utilizado para instanciar pedidos de forma controlada.
        /// </summary>
        internal PedidoTransacao(long id, double valorProposto, TipoAnuncio tipoAnuncio, long anuncioId, long compradorId, long vendedorId)
        {
            this.Id = id;
            this.ValorProposto = valorProposto;
            this.TipoAnuncio = tipoAnuncio;
            this.AnuncioId = anuncioId;
            this.CompradorId = compradorId;
            this.VendedorId = vendedorId;
        }

        /// <summary>
        /// Cria um novo pedido de transação com base nos dados fornecidos.
        /// O pedido é inicializado com estado PENDENTE e definido como do tipo indicado (venda ou aluguer).
        /// </summary>
        public static PedidoTransacao CriarPedido(double valor, TipoAnuncio tipoAnuncio, long anuncioId, 
            long compradorId, long vendedorId, long destinatarioId, long remetenteId, long conversaId, int? diasDeAluguel = null)
        {
            var PedidoTransacao = new PedidoTransacao
            {
                ValorProposto = valor,
                DataCriacao = DateTime.UtcNow,
                TipoAnuncio = tipoAnuncio,
                EstadoPedido = EstadoPedido.PENDENTE,
                AnuncioId = anuncioId,
                CompradorId = compradorId,
                VendedorId = vendedorId,
                DestinatarioId = destinatarioId,
                RemetenteId = remetenteId,
                ConversaId = conversaId,
                DiasDeAluguel = tipoAnuncio == TipoAnuncio.ALUGUER ? diasDeAluguel : null,
            };

            return PedidoTransacao;
        }

        /// <summary>
        /// Altera manualmente o estado do pedido.
        /// </summary>
        /// <param name="pedido">Novo estado a atribuir.</param>
        public void AlterarEstado(EstadoPedido pedido)
        {
            this.EstadoPedido = pedido;
        }

        /// <summary>
        /// Aceita o pedido atual, validando se, o pedido ainda se encontra pendentee se o
        /// utilizador que executa a ação é o destinatário. Caso contrário, é 
        /// lançada uma exceção de operação inválida.
        /// </summary>
        /// <param name="utilizadorId">Identificador do utilizador que tenta aceitar o pedido.</param>
        /// <exception cref="InvalidOperationException">
        /// Lançada se o pedido já tiver sido processado ou se o utilizador não for o destinatário.
        /// </exception>
        public void Aceitar(long utilizadorId)
        {
            if (EstadoPedido != EstadoPedido.PENDENTE)
            {
                throw new InvalidOperationException("O pedido já foi processado.");
            }

            if (utilizadorId != DestinatarioId)
            {
                throw new InvalidOperationException("Apenas o destinatário pode aceitar o pedido.");
            }

            EstadoPedido = EstadoPedido.ACEITE;
        }

        /// <summary>
        /// Rejeita o pedido atual, garantindo que apenas o destinatário pode rejeitar,
        /// e que o pedido se encontra pendente. Caso contrário, é lançada uma exceção.
        /// </summary>
        /// <param name="utilizadorId">Identificador do utilizador que tenta rejeitar o pedido.</param>
        /// <exception cref="InvalidOperationException">
        /// Lançada se o pedido já tiver sido processado ou se o utilizador não for o destinatário.
        /// </exception>
        public void Rejeitar(long utilizadorId)
        {
            if (EstadoPedido != EstadoPedido.PENDENTE)
            {
                throw new InvalidOperationException("O pedido já foi processado.");
            }

            if (utilizadorId != DestinatarioId)
            {
                throw new InvalidOperationException("Apenas o destinatário pode aceitar o pedido.");
            }

            EstadoPedido = EstadoPedido.REJEITADO;
        }

        /// <summary>
        /// Cancela o pedido atual, apenas se o estado for PENDENTE.
        /// Caso contrário, lança uma exceção, evitando cancelamentos indevidos.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Lançada se o pedido já tiver sido aceite, rejeitado ou cancelado anteriormente.
        /// </exception>
        public void Cancelar()
        {
            if (EstadoPedido != EstadoPedido.PENDENTE)
            {
                throw new InvalidOperationException("Apenas pedidos pendentes podem ser cancelados.");
            }

            EstadoPedido = EstadoPedido.CANCELADO;
        }

    }
}

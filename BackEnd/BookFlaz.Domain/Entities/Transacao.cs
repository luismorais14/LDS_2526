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
    public class Transacao
    {
        private const double TAXA_CONVERSAO = 0.05;
        private const double DESCONTO_MAXIMO = 0.5;
        private const int PONTOS_MINIMOS = 100;

        [Required]
        public long Id { get; private set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor total deve ser maior ou igual a 0.1.")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "O valor deve ter no máximo duas casas decimais.")]
        public double ValorFinal { get; private set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Os pontos usados não podem ser negativos.")]
        public int PontosUsados { get; private set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "O valor do desconto não pode ser negativo.")]
        public double ValorDesconto { get; private set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DataCriacao { get; private set; } = DateTime.Now;

        [Required]
        public EstadoTransacao EstadoTransacao { get; private set; }

        [Required]
        [ForeignKey("PedidoTransacao")]
        public long PedidoId { get; private set; }

        [Required]
        [ForeignKey("Cliente")]
        public long CompradorId { get; private set; }

        [Required]
        [ForeignKey("Cliente")]
        public long VendedorId { get; private set; }

        internal Transacao() { }



        public static Transacao CriarTransacao(double valorTotal, int pontosUsados, long pedidoId, long compradorId, long vendedorId)
        {
            if (valorTotal <= 0)
            {
                throw new ArgumentException("O valor total deve ser superior a zero.");
            }

            if (pontosUsados < 0)
            {
                throw new ArgumentException("Os pontos usados não podem ser negativos.");
            }

            if (pontosUsados == 0)
            {
                var transacao = new Transacao
                {
                    ValorFinal = valorTotal,
                    PontosUsados = 0,
                    ValorDesconto = 0,
                    DataCriacao = DateTime.Now,
                    EstadoTransacao = EstadoTransacao.PENDENTE,
                    PedidoId = pedidoId,
                    CompradorId = compradorId,
                    VendedorId = vendedorId
                };

                return transacao;
            }

            if (pontosUsados < PONTOS_MINIMOS)
            {
                throw new InvalidOperationException($"É necessário um mínimo de {PONTOS_MINIMOS} pontos para aplicar desconto.");
            }

            double valorDesconto = pontosUsados * TAXA_CONVERSAO;
            double descontoMaximo = valorTotal * DESCONTO_MAXIMO;

            if (valorDesconto > descontoMaximo)
            {
                valorDesconto = descontoMaximo;
            }

            double valorFinal = valorTotal - valorDesconto;

            if (valorFinal < 0)
            {
                valorFinal = 0;
            }

            var transacaoFinal = new Transacao
            {
                ValorFinal = valorFinal,
                PontosUsados = pontosUsados,
                ValorDesconto = valorDesconto,
                DataCriacao = DateTime.Now,
                EstadoTransacao = EstadoTransacao.PENDENTE,
                PedidoId = pedidoId,
                CompradorId = compradorId,
                VendedorId = vendedorId
            };

            return transacaoFinal;
        }

        public void AtualizarEstado(EstadoTransacao novoEstado)
        {
            EstadoTransacao = novoEstado;
        }

        public void Cancelar(long utilizadorId)
        {
            if (EstadoTransacao != EstadoTransacao.PENDENTE)
            {
                throw new InvalidOperationException("A transação já foi processada e não pode ser cancelada.");
            }

            if (utilizadorId != CompradorId && utilizadorId != VendedorId)
            {
                throw new InvalidOperationException("Apenas o comprador ou o vendedor podem cancelar a transação.");
            }

            EstadoTransacao = EstadoTransacao.CANCELADA;
        }

        public void ConfirmarRececaoComprador(long utilizadorId, TipoAnuncio tipoAnuncio)
        {
            if (EstadoTransacao != EstadoTransacao.PENDENTE)
            {
                throw new InvalidOperationException("A transação já foi processada.");
            }

            if (utilizadorId != CompradorId)
            {
                throw new InvalidOperationException("Apenas o comprador pode confirmar esta transação.");
            }

            if (tipoAnuncio != TipoAnuncio.VENDA && tipoAnuncio != TipoAnuncio.DOACAO)
            {
                throw new InvalidOperationException("O comprador só pode confirmar transações de compra ou doação.");
            }

            EstadoTransacao = EstadoTransacao.CONCLUIDA;
        }

        public void ConfirmarRececaoVendedor(long utilizadorId, TipoAnuncio tipoAnuncio)
        {
            if (EstadoTransacao != EstadoTransacao.PENDENTE)
            {
                throw new InvalidOperationException("A transação já foi processada.");
            }

            if (utilizadorId != VendedorId)
            {
                throw new InvalidOperationException("Apenas o vendedor pode confirmar esta transação.");
            }

            if (tipoAnuncio != TipoAnuncio.ALUGUER)
            {
                throw new InvalidOperationException("O vendedor só pode confirmar transações de aluguer.");
            }

            EstadoTransacao = EstadoTransacao.CONCLUIDA;
        }
    

        public void ConfirmarDevolucaoVendedor(long utilizadorId, TipoAnuncio tipoAnuncio)
        {
            if (EstadoTransacao != EstadoTransacao.DEVOLUCAO_PENDENTE)
            {
                throw new InvalidOperationException("A transação não está em devolução pendente.");
            }

            if (utilizadorId != VendedorId)
            {
                throw new InvalidOperationException("Apenas o vendedor pode confirmar esta devolução.");
            }

            if (tipoAnuncio != TipoAnuncio.ALUGUER)
            {
                throw new InvalidOperationException("O vendedor só pode confirmar devoluções de aluguer.");
            }

            EstadoTransacao = EstadoTransacao.CONCLUIDA;
        }
    }
}

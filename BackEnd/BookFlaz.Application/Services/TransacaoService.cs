using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection; 
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using BookFlaz.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookFlaz.Application.Services
{
	public class TransacaoService : ITransacaoService
	{
		private readonly ITransacaoRepository _transacaoRepo;
		private readonly IPedidoTransacaoRepository _pedidoRepo;
		private readonly IAnuncioRepository _anuncioRepo;
		private readonly IClienteRepository _clienteRepo;
		private readonly IDevolucaoRepository _devolucaoRepo;
		private readonly IPontosService _pontosService;
		private readonly INotificacaoService _notiService;

		public TransacaoService(
			ITransacaoRepository transacaoRepository,
			IPedidoTransacaoRepository pedidoRepository,
			IAnuncioRepository anuncioRepository,
			IClienteRepository clienteRepository,
			IDevolucaoRepository devolucaoRepository,
			IPontosService pontosService,
			INotificacaoService notificacaoService)
		{
			_transacaoRepo = transacaoRepository;
			_pedidoRepo = pedidoRepository;
			_anuncioRepo = anuncioRepository;
			_clienteRepo = clienteRepository;
			_devolucaoRepo = devolucaoRepository;
			_pontosService = pontosService;
			_notiService = notificacaoService;
		}

		/// <summary>
		/// Cria uma nova transação associada a um pedido existente.
		/// </summary>
		/// <param name="dto">Dados necessários para criar a transação.</param>
		/// <param name="userId">ID do utilizador </param>
		/// <returns>Detalhes da transação criada.</returns>
		public async Task<TransacaoDTO> CriarTransacaoAsync(CriarTransacaoDTO dto, long userId)
		{
			if (dto == null)
				throw new ArgumentNullException(nameof(dto));

			var pedido = await _pedidoRepo.ObterPorIdAsync(dto.PedidoId);

			if (pedido == null)
				throw new KeyNotFoundException("Pedido não encontrado.");

			if (pedido.CompradorId != userId)
			{
				throw new BusinessException("Apenas o comprador pode criar a transação");
			}

			var transacaoExistente = await _transacaoRepo.ObterPorPedidoIdAsync(dto.PedidoId);

			if (transacaoExistente != null)
			{
				throw new BusinessException("Já existe uma transação associada a este pedido.");
			}

			try
			{
				if (dto.PontosUsados > 0)
				{
					var saldoPontos = await _pontosService.ObterPontosAsync(pedido.CompradorId);
					if (dto.PontosUsados > saldoPontos)
					{
						throw new BusinessException("Saldo de pontos insuficiente.");
					}

					await _pontosService.RemoverPontos(pedido.CompradorId, dto.PontosUsados, pedido.Id);
				}

				var transacao = BookFlaz.Domain.Entities.Transacao.CriarTransacao(
					valorTotal: pedido.ValorProposto,
					pontosUsados: dto.PontosUsados,
					pedidoId: pedido.Id,
					compradorId: pedido.CompradorId,
					vendedorId: pedido.VendedorId
				);

				await _transacaoRepo.AdicionarAsync(transacao);

				return new TransacaoDTO
				{
					Id = transacao.Id,
					DataTransacao = transacao.DataCriacao,
					CompradorId = (int)transacao.CompradorId,
					VendedorId = (int)transacao.VendedorId,
					PedidoId = transacao.PedidoId,
					ValorFinal = (decimal)transacao.ValorFinal,
					PontosUsados = transacao.PontosUsados,
					ValorDesconto = (decimal)transacao.ValorDesconto,
					EstadoTransacao = transacao.EstadoTransacao.ToString(),
				};
			}
			catch (InvalidOperationException ex)
			{
				throw new BusinessException(ex.Message);
			}
			catch (Exception)
			{
				throw new ApplicationException("Erro ao criar a transação.");
			}
		}

		/// <summary>
		/// Confirma a receção de uma transação por parte do comprador. 
		/// </summary>
		/// <param name="transacaoId"> Id da transação </param>
		/// <param name="compradorId"> Id do comprador </param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="UnauthorizedActionException"></exception>
		/// <exception cref="BusinessException"></exception>
		/// <exception cref="ApplicationException"></exception>

		public async Task ConfirmarRececaoCompradorAsync(long transacaoId, long compradorId)
		{
			var transacao = await _transacaoRepo.ObterPorIdAsync(transacaoId);
			if (transacao == null)
				throw new KeyNotFoundException("Transação não encontrada.");
			if (transacao.CompradorId != compradorId)
				throw new UnauthorizedActionException("Apenas o comprador pode confirmar a receção.");

			var pedido = await _pedidoRepo.ObterPorIdAsync(transacao.PedidoId) ?? throw new KeyNotFoundException("Pedido associado à transação não encontrado.");

			try
			{
				transacao.ConfirmarRececaoComprador(compradorId, pedido.TipoAnuncio);
				await _transacaoRepo.AtualizarAsync(transacao);

				await _notiService.CriarNotificacaoAsync("Transação concluída do anúncio " + pedido.AnuncioId, TipoNotificacao.TRANSACAO, pedido.RemetenteId);
				await _notiService.CriarNotificacaoAsync("Transação concluída do anúncio " + pedido.AnuncioId, TipoNotificacao.TRANSACAO, pedido.DestinatarioId);

				if (pedido.TipoAnuncio != TipoAnuncio.DOACAO)
				{
					int pontosComprador = (int)(transacao.ValorFinal);
					await _pontosService.AdicionarPontos(pedido.CompradorId, pontosComprador, transacao.Id);
					await _pontosService.AdicionarPontos(pedido.VendedorId, pontosComprador, transacao.Id);
				}
			}
			catch (InvalidOperationException ex)
			{
				throw new BusinessException(ex.Message);
			}
			catch (Exception)
			{
				throw new ApplicationException("Erro ao confirmar a receção da transação.");
			}
		}

		/// <summary>
		/// Cancela uma transação pendente por parte do comprador.
		/// </summary>
		/// <param name="transacaoId"> Id da transação </param>
		/// <param name="compradorId">Id do comprador </param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="UnauthorizedActionException"></exception>
		/// <exception cref="BusinessException"></exception>
		/// <exception cref="ApplicationException"></exception>
		public async Task CancelarTransacaoAsync(long transacaoId, long compradorId)
		{
			var transacao = await _transacaoRepo.ObterPorIdAsync(transacaoId);
			if (transacao == null)
				throw new KeyNotFoundException("Transação não encontrada.");
			if (transacao.CompradorId != compradorId)
				throw new UnauthorizedActionException("Apenas o comprador pode cancelar a transação.");
			if (transacao.EstadoTransacao != EstadoTransacao.PENDENTE)
				throw new BusinessException("Só é possível cancelar transações pendentes.");

			try
			{
				transacao.AtualizarEstado(EstadoTransacao.CANCELADA);
				await _transacaoRepo.AtualizarAsync(transacao);
				if (transacao.PontosUsados > 0)
				{
					await _pontosService.AdicionarPontos(compradorId, transacao.PontosUsados, transacao.Id);
				}

				var pedido = await _pedidoRepo.ObterPorIdAsync(transacao.PedidoId);
				if (pedido != null)
				{
					var anuncio = await _anuncioRepo.ObterPorIdAsync(pedido.AnuncioId);
					if (anuncio != null)
					{
						anuncio.AtualizarEstadoAnuncio(EstadoAnuncio.ATIVO);
						await _anuncioRepo.Atualizar(anuncio);
					}
				}
			}
			catch (InvalidOperationException ex)
			{
				throw new BusinessException(ex.Message);
			}
			catch (Exception)
			{
				throw new ApplicationException("Erro ao cancelar a transação.");
			}
		}

		/// <summary>
		/// Regista uma devolução para uma transação de aluguer por parte do comprador.
		/// </summary>
		/// <param name="transacaoId">Id da transação </param>
		/// <param name="compradorId"> Id do comprador </param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="UnauthorizedActionException"></exception>
		/// <exception cref="BusinessException"></exception>
		/// <exception cref="ApplicationException"></exception>

		public async Task RegistarDevolucaoAsync(long transacaoId, long compradorId)
		{
			var transacao = await _transacaoRepo.ObterPorIdAsync(transacaoId)
				?? throw new NotFoundException("Transação não encontrada.");

			if (transacao.CompradorId != compradorId)
				throw new UnauthorizedActionException("Apenas o comprador pode registar uma devolução.");

			var pedido = await _pedidoRepo.ObterPorIdAsync(transacao.PedidoId)
				?? throw new NotFoundException("Pedido associado à transação não encontrado.");

			if (pedido.TipoAnuncio != TipoAnuncio.ALUGUER)
				throw new BusinessException("Só é possível registar devoluções para alugueres.");

			try
			{
				transacao.AtualizarEstado(EstadoTransacao.DEVOLUCAO_PENDENTE);
				await _transacaoRepo.AtualizarAsync(transacao);

				var devolucao = Devolucao.CriarDevolucao(transacaoId, compradorId);
				await _devolucaoRepo.AdicionarAsync(devolucao);
			}
			catch (InvalidOperationException ex)
			{
				throw new BusinessException(ex.Message);
			}
			catch (Exception)
			{
				throw new ApplicationException("Erro ao registar devolução.");
			}
		}


		/// <summary>
		/// Confirma a devolução de uma transação de aluguer por parte do vendedor.
		/// </summary>
		/// <param name="transacaoId"> Id da transação </param>
		/// <param name="vendedorId"> Id do vendedor </param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="UnauthorizedActionException"></exception>
		/// <exception cref="BusinessException"></exception>
		/// <exception cref="ApplicationException"></exception>
		public async Task ConfirmarDevolucaoAsync(long transacaoId, long vendedorId)
		{
			var transacao = await _transacaoRepo.ObterPorIdAsync(transacaoId)
				?? throw new KeyNotFoundException("Transação não encontrada.");
			if (transacao.VendedorId != vendedorId)
				throw new UnauthorizedActionException("Apenas o vendedor pode confirmar a devolução.");

			var pedido = await _pedidoRepo.ObterPorIdAsync(transacao.PedidoId)
				?? throw new KeyNotFoundException("Pedido associado à transação não encontrado.");

			try
			{
				transacao.ConfirmarDevolucaoVendedor(vendedorId, pedido.TipoAnuncio);
				await _transacaoRepo.AtualizarAsync(transacao);

				await _notiService.CriarNotificacaoAsync("Transação concluída do anúncio " + pedido.AnuncioId, TipoNotificacao.TRANSACAO, pedido.RemetenteId);
				await _notiService.CriarNotificacaoAsync("Transação concluída do anúncio " + pedido.AnuncioId, TipoNotificacao.TRANSACAO, pedido.DestinatarioId);

				var anuncio = await _anuncioRepo.ObterPorIdAsync(pedido.AnuncioId);
				if (anuncio != null)
				{
					anuncio.AtualizarEstadoAnuncio(EstadoAnuncio.ATIVO);
					await _anuncioRepo.Atualizar(anuncio);
				}


				var devolucao = await _devolucaoRepo.ObterPorTransacaoIdAsync(transacaoId);
				if (devolucao != null)
				{
					devolucao.Confirmar(vendedorId, DateTime.Now);
					await _devolucaoRepo.AtualizarAsync(devolucao);
				}

				var pontos = (int)transacao.ValorFinal;
				if (pontos > 0)
				{
					await _pontosService.AdicionarPontos(transacao.CompradorId, pontos, transacao.Id);
					await _pontosService.AdicionarPontos(transacao.VendedorId, pontos, transacao.Id);
				}

			}
			catch (InvalidOperationException ex)
			{
				throw new BusinessException(ex.Message);
			}
			catch (Exception)
			{
				throw new ApplicationException("Erro ao confirmar a devolução da transação.");
			}
		}


		public async Task<List<TransacaoResumoDTO>> ObterRegistoAsync(long utilizadorId, TransacaoFiltroDTO filtro)
		{
			List<Transacao> transacoes = null;

			if (filtro != null)
			{
				transacoes = await _transacaoRepo.ObterDoUtilizadorAsync(
					utilizadorId,
					filtro.De,
					filtro.Ate,
					filtro.Estado
				);
			}
			else
			{
				transacoes = await _transacaoRepo.ObterPorClienteIdAsync(utilizadorId);
			}

			Console.WriteLine($"Transações obtidas do repositório: {transacoes?.Count ?? 0}");

			var lista = new List<TransacaoResumoDTO>();

			foreach (var t in transacoes ?? new List<Transacao>())
			{
				
				if (filtro != null && !string.IsNullOrEmpty(filtro.Papel))
				{
					if (filtro.Papel.Equals("comprador", StringComparison.OrdinalIgnoreCase) && t.CompradorId != utilizadorId)
						continue;
					if (filtro.Papel.Equals("vendedor", StringComparison.OrdinalIgnoreCase) && t.VendedorId != utilizadorId)
						continue;
				}

				var pedido = await _pedidoRepo.ObterPorIdAsync(t.PedidoId);

				if (filtro != null && filtro.Tipo.HasValue)
				{
					if (pedido == null || pedido.TipoAnuncio != filtro.Tipo.Value)
						continue;
				}

				string? titulo = null;
				string? imagem = null;
				decimal? preco = null;
				TipoAnuncio? tipo = pedido?.TipoAnuncio;

				if (pedido != null)
				{
					var anuncio = await _anuncioRepo.ObterPorIdAsync(pedido.AnuncioId);
					if (anuncio != null)
					{
						titulo = anuncio.Livro?.Titulo;
						imagem = !string.IsNullOrEmpty(anuncio.Imagens)
							? anuncio.Imagens.Split(';').FirstOrDefault()
							: null;
						preco = anuncio.Preco;
					}
				}

				var isComprador = t.CompradorId == utilizadorId;

				lista.Add(new TransacaoResumoDTO
				{
					Id = t.Id,
					Data = t.DataCriacao,
					Estado = t.EstadoTransacao.ToString(),
					PedidoId = t.PedidoId,

					AnuncioId = pedido?.AnuncioId,
					TituloAnuncio = titulo,
					ImagemAnuncio = imagem,
					Preco = preco,
					TipoAnuncio = tipo,

					OutroUtilizadorId = isComprador ? t.VendedorId : t.CompradorId,

					Papel = isComprador ? "COMPRADOR" : "VENDEDOR",

					ValorFinal = (decimal)t.ValorFinal,
					PontosUsados = t.PontosUsados,
					ValorDesconto = (decimal)t.ValorDesconto
				});
			}

			return lista;
		}
	}
}

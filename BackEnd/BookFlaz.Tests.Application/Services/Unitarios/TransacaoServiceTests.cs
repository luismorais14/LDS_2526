﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using Moq;
using Xunit;

namespace BookFlaz.Tests.Application.Services.Unitarios
{
	public class TransacaoServiceTests
	{
		private readonly Mock<ITransacaoRepository> _transacaoRepoMock;
		private readonly Mock<IPedidoTransacaoRepository> _pedidoRepoMock;
		private readonly Mock<IAnuncioRepository> _anuncioRepoMock;
		private readonly Mock<IClienteRepository> _clienteRepoMock;
		private readonly Mock<IDevolucaoRepository> _devolucaoRepoMock;
		private readonly Mock<IPontosService> _pontosServMock;
		private readonly Mock<INotificacaoService> _notiServMock;

		private readonly TransacaoService _service;

		public TransacaoServiceTests()
		{
			_transacaoRepoMock = new Mock<ITransacaoRepository>();
			_pedidoRepoMock = new Mock<IPedidoTransacaoRepository>();
			_anuncioRepoMock = new Mock<IAnuncioRepository>();
			_clienteRepoMock = new Mock<IClienteRepository>();
			_devolucaoRepoMock = new Mock<IDevolucaoRepository>();
			_pontosServMock = new Mock<IPontosService>();
			_notiServMock = new Mock<INotificacaoService>();

			_service = new TransacaoService(
				_transacaoRepoMock.Object,
				_pedidoRepoMock.Object,
				_anuncioRepoMock.Object,
				_clienteRepoMock.Object,
				_devolucaoRepoMock.Object,
				_pontosServMock.Object,
				_notiServMock.Object
			);
		}


		private Anuncio CriarAnuncioValido(TipoAnuncio tipo = TipoAnuncio.VENDA, long id = 7)
		{
			var anuncio = Anuncio.CriarAnuncio(30, 123, 1, 2, EstadoLivro.USADO, tipo, "img.jpg");
			typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncio, Livro.AdicionarLivro(123, "OLAOLA", "RUI"));
			typeof(Anuncio).GetProperty("Id")!.SetValue(anuncio, id);
			return anuncio;
		}

		private PedidoTransacao CriarPedidoValido(
			TipoAnuncio tipo = TipoAnuncio.VENDA,
			double valor = 30,
			long compradorId = 1,
			long vendedorId = 2,
			long anuncioId = 7
		)
		{
			var pedido = PedidoTransacao.CriarPedido(
				valor: valor,
				tipoAnuncio: tipo,
				anuncioId: anuncioId,
				compradorId: compradorId,
				vendedorId: vendedorId,
				destinatarioId: vendedorId,
				remetenteId: compradorId,
				conversaId: 1,
				diasDeAluguel: tipo == TipoAnuncio.ALUGUER ? 5 : (int?)null
			);
			typeof(PedidoTransacao).GetProperty("Id")!.SetValue(pedido, 101L);
			return pedido;
		}

		private Transacao CriarTransacaoPendente(PedidoTransacao p, int pontosUsados = 0)
		{
			var t = Transacao.CriarTransacao(
				valorTotal: p.ValorProposto,
				pontosUsados: pontosUsados,
				pedidoId: p.Id,
				compradorId: p.CompradorId,
				vendedorId: p.VendedorId
			);
			typeof(Transacao).GetProperty("Id")!.SetValue(t, 1001L);
			return t;
		}



		[Fact]
		public async Task CriarTransacaoAsync_ComDadosValidos_DeveCriarTransacaoSemErro()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, valor: 100, compradorId: 10, vendedorId: 20, anuncioId: 7);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			_transacaoRepoMock
				.Setup(x => x.AdicionarAsync(It.IsAny<Transacao>()))
				.Callback<Transacao>(t => typeof(Transacao).GetProperty("Id")!.SetValue(t, 333L))
				.Returns(Task.CompletedTask);

			var dto = new CriarTransacaoDTO { PedidoId = pedido.Id, PontosUsados = 0 };

			var result = await _service.CriarTransacaoAsync(dto, 10);

			Assert.NotNull(result);
			Assert.Equal(333, result.Id);
			Assert.Equal(pedido.Id, result.PedidoId);
			Assert.Equal(10, result.CompradorId);
			Assert.Equal(20, result.VendedorId);
			Assert.Equal(100m, result.ValorFinal);

			_transacaoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<Transacao>()), Times.Once);
			_pontosServMock.VerifyNoOtherCalls();
		}

		[Fact]
		public async Task CriarTransacaoAsync_PedidoInexistente_DeveLancarNotFound()
		{
			var dto = new CriarTransacaoDTO { PedidoId = 999, PontosUsados = 0 };
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(dto.PedidoId)).ReturnsAsync((PedidoTransacao)null!);

			var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CriarTransacaoAsync(dto, 1));

			Assert.Equal("Pedido não encontrado.", ex.Message);
			_transacaoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<Transacao>()), Times.Never);
		}

		[Fact]
		public async Task CriarTransacaoAsync_ComPontosAbaixoMinimo_DeveLancarBusiness()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, valor: 100, compradorId: 10, vendedorId: 20);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			_pontosServMock.Setup(s => s.ObterPontosAsync(10)).ReturnsAsync(9999);

			var dto = new CriarTransacaoDTO { PedidoId = pedido.Id, PontosUsados = 50 };

			var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.CriarTransacaoAsync(dto, 10));
			Assert.Contains("mínimo", ex.Message, StringComparison.OrdinalIgnoreCase);

			_transacaoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<Transacao>()), Times.Never);
		}

		[Fact]
		public async Task CriarTransacaoAsync_ComPontosValidos_DeveDebitarPontosDoComprador()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, valor: 200, compradorId: 10, vendedorId: 20);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			_pontosServMock.Setup(s => s.ObterPontosAsync(10)).ReturnsAsync(10_000);
			_pontosServMock.Setup(s => s.RemoverPontos(10, 200, It.IsAny<long>())).Returns(Task.CompletedTask);

			_transacaoRepoMock
				.Setup(x => x.AdicionarAsync(It.IsAny<Transacao>()))
				.Callback<Transacao>(t => typeof(Transacao).GetProperty("Id")!.SetValue(t, 777L))
				.Returns(Task.CompletedTask);

			var dto = new CriarTransacaoDTO { PedidoId = pedido.Id, PontosUsados = 200 };

			var res = await _service.CriarTransacaoAsync(dto, 10);

			Assert.NotNull(res);
			_pontosServMock.Verify(s => s.RemoverPontos(10, 200, It.IsAny<long>()), Times.Once);
		}



		[Fact]
		public async Task ConfirmarRececaoCompradorAsync_TransacaoInexistente_DeveLancarNotFound()
		{
			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync((Transacao)null!);

			await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ConfirmarRececaoCompradorAsync(1, 10));
		}

		[Fact]
		public async Task ConfirmarRececaoCompradorAsync_CompradorDiferente_DeveLancarUnauthorized()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, valor: 50, compradorId: 10, vendedorId: 20);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			await Assert.ThrowsAsync<UnauthorizedActionException>(() => _service.ConfirmarRececaoCompradorAsync(t.Id, 999));

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Never);
		}

		[Fact]
		public async Task ConfirmarRececaoCompradorAsync_PedidoInexistente_DeveLancarNotFound()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync((PedidoTransacao)null!);

			var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ConfirmarRececaoCompradorAsync(t.Id, pedido.CompradorId));
			Assert.Equal("Pedido associado à transação não encontrado.", ex.Message);

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Never);
		}

		[Fact]
		public async Task ConfirmarRececaoCompradorAsync_TipoInvalido_DeveLancarBusiness()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.ALUGUER, valor: 50, compradorId: 10, vendedorId: 20);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.ConfirmarRececaoCompradorAsync(t.Id, 10));
			Assert.Contains("comprador só pode confirmar", ex.Message, StringComparison.OrdinalIgnoreCase);

			_transacaoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<Transacao>()), Times.Never);

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Never);
		}

		[Fact]
		public async Task ConfirmarRececaoCompradorAsync_ComDadosValidos_DeveConcluir_ECreditarPontosACompradorEVendedor()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, valor: 50, compradorId: 10, vendedorId: 20);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);
			_transacaoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);

			_pontosServMock.Setup(s => s.AdicionarPontos(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>()))
						   .Returns(Task.CompletedTask);

			var ex = await Record.ExceptionAsync(() => _service.ConfirmarRececaoCompradorAsync(t.Id, 10));

			Assert.Null(ex);

			_transacaoRepoMock.Verify(
				x => x.AtualizarAsync(It.Is<Transacao>(tt => tt.EstadoTransacao == EstadoTransacao.CONCLUIDA)),
				Times.Once
			);

			var pontosEsperados = (int)t.ValorFinal; // 1 ponto por euro
			_pontosServMock.Verify(s => s.AdicionarPontos(10, pontosEsperados, t.Id), Times.Once);
			_pontosServMock.Verify(s => s.AdicionarPontos(20, pontosEsperados, t.Id), Times.Once);

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Exactly(2));
		}

		[Fact]
		public async Task ConfirmarRececaoCompradorAsync_Doacao_NaoCreditaPontos()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.DOACAO, valor: 50, compradorId: 10, vendedorId: 20);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(r => r.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);
			_transacaoRepoMock.Setup(r => r.AtualizarAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);

			await _service.ConfirmarRececaoCompradorAsync(t.Id, 10);

			_pontosServMock.Verify(s => s.AdicionarPontos(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>()), Times.Never);

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Exactly(2));
		}


		[Fact]
		public async Task CancelarTransacaoAsync_TransacaoInexistente_DeveLancarNotFound()
		{
			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync((Transacao)null!);
			await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CancelarTransacaoAsync(1, 10));
		}

		[Fact]
		public async Task CancelarTransacaoAsync_CompradorErrado_DeveLancarUnauthorized()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, 50, 10, 20, 7);
			var t = CriarTransacaoPendente(pedido);
			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);

			await Assert.ThrowsAsync<UnauthorizedActionException>(() => _service.CancelarTransacaoAsync(t.Id, 999));
		}

		[Fact]
		public async Task CancelarTransacaoAsync_NaoPendente_DeveLancarBusiness()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, 50, 10, 20, 7);
			var t = CriarTransacaoPendente(pedido);
			t.AtualizarEstado(EstadoTransacao.CONCLUIDA);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);

			await Assert.ThrowsAsync<BusinessException>(() => _service.CancelarTransacaoAsync(t.Id, 10));
		}

		[Fact]
		public async Task CancelarTransacaoAsync_Pendente_DeveCancelarEReativarAnuncio()
		{
			var anuncio = CriarAnuncioValido(TipoAnuncio.VENDA, id: 7);
			anuncio.AtualizarEstadoAnuncio(EstadoAnuncio.INDISPONIVEL);

			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, 50, 10, 20, anuncio.Id);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);
			_anuncioRepoMock.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);

			_transacaoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);
			_anuncioRepoMock.Setup(x => x.Atualizar(It.IsAny<Anuncio>())).Returns(Task.CompletedTask);

			var ex = await Record.ExceptionAsync(() => _service.CancelarTransacaoAsync(t.Id, 10));

			Assert.Null(ex);
			_transacaoRepoMock.Verify(
				x => x.AtualizarAsync(It.Is<Transacao>(tt => tt.EstadoTransacao == EstadoTransacao.CANCELADA)),
				Times.Once
			);
			_anuncioRepoMock.Verify(x => x.Atualizar(It.Is<Anuncio>(a => a.EstadoAnuncio == EstadoAnuncio.ATIVO)), Times.Once);
		}

		[Fact]
		public async Task CancelarTransacaoAsync_ComPontosUsados_DeveEstornarAoComprador()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, valor: 100, compradorId: 10, vendedorId: 20);
			var t = CriarTransacaoPendente(pedido, pontosUsados: 150);

			_transacaoRepoMock.Setup(r => r.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_transacaoRepoMock.Setup(r => r.AtualizarAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);
			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			var anuncio = CriarAnuncioValido(TipoAnuncio.VENDA, pedido.AnuncioId);
			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(pedido.AnuncioId)).ReturnsAsync(anuncio);
			_anuncioRepoMock.Setup(r => r.Atualizar(It.IsAny<Anuncio>())).Returns(Task.CompletedTask);

			_pontosServMock.Setup(s => s.AdicionarPontos(10, 150, t.Id)).Returns(Task.CompletedTask);

			await _service.CancelarTransacaoAsync(t.Id, compradorId: 10);

			_pontosServMock.Verify(s => s.AdicionarPontos(10, 150, t.Id), Times.Once);
		}


		[Fact]
		public async Task RegistarDevolucaoAsync_TransacaoInexistente_DeveLancarNotFound()
		{
			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(1))
							  .ReturnsAsync((Transacao)null!);

			await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.RegistarDevolucaoAsync(1, 10));
		}

		[Fact]
		public async Task RegistarDevolucaoAsync_PedidoInexistente_DeveLancarNotFound()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.ALUGUER);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id))
						   .ReturnsAsync((PedidoTransacao)null!);

			var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
				_service.RegistarDevolucaoAsync(t.Id, compradorId: pedido.CompradorId));

			Assert.Equal("Pedido associado à transação não encontrado.", ex.Message);
		}

		[Fact]
		public async Task RegistarDevolucaoAsync_CompradorErrado_DeveLancarUnauthorized()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.ALUGUER, 30, 10, 20, 7);
			var t = CriarTransacaoPendente(pedido);
			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);

			await Assert.ThrowsAsync<UnauthorizedActionException>(() => _service.RegistarDevolucaoAsync(t.Id, compradorId: 999));
		}



		[Fact]
		public async Task RegistarDevolucaoAsync_TipoNaoAluguer_DeveLancarBusiness()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, 30, 10, 20);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			await Assert.ThrowsAsync<BusinessException>(() => _service.RegistarDevolucaoAsync(t.Id, compradorId: 10));
		}

		[Fact]
		public async Task RegistarDevolucaoAsync_ComDadosValidos_DeveMarcarDevolucaoPendenteECriarDevolucao()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.ALUGUER, 30, 10, 20, 7);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			_transacaoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);
			_devolucaoRepoMock.Setup(x => x.AdicionarAsync(It.IsAny<Devolucao>())).Returns(Task.CompletedTask);

			var ex = await Record.ExceptionAsync(() => _service.RegistarDevolucaoAsync(t.Id, compradorId: 10));

			Assert.Null(ex);
			_transacaoRepoMock.Verify(
				x => x.AtualizarAsync(It.Is<Transacao>(tt => tt.EstadoTransacao == EstadoTransacao.DEVOLUCAO_PENDENTE)),
				Times.Once
			);
			_devolucaoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<Devolucao>()), Times.Once);
		}



		[Fact]
		public async Task ConfirmarDevolucaoAsync_TransacaoInexistente_DeveLancarNotFound()
		{
			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync((Transacao)null!);
			await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ConfirmarDevolucaoAsync(1, vendedorId: 20));
		}

		[Fact]
		public async Task ConfirmarDevolucaoAsync_VendedorErrado_DeveLancarUnauthorized()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.ALUGUER, 30, 10, 20, 7);
			var t = CriarTransacaoPendente(pedido);
			t.AtualizarEstado(EstadoTransacao.DEVOLUCAO_PENDENTE);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			await Assert.ThrowsAsync<UnauthorizedActionException>(() => _service.ConfirmarDevolucaoAsync(t.Id, vendedorId: 999));

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Never);
		}

		[Fact]
		public async Task ConfirmarDevolucaoAsync_PedidoInexistente_DeveLancarNotFound()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.ALUGUER, 30, 10, 20, 7);
			var t = CriarTransacaoPendente(pedido);
			t.AtualizarEstado(EstadoTransacao.DEVOLUCAO_PENDENTE);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync((PedidoTransacao)null!);

			var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ConfirmarDevolucaoAsync(t.Id, vendedorId: 20));
			Assert.Equal("Pedido associado à transação não encontrado.", ex.Message);

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Never);
		}

		[Fact]
		public async Task ConfirmarDevolucaoAsync_EstadoNaoDevolucaoPendenteOuTipoInvalido_DeveLancarBusiness()
		{
			var pedido = CriarPedidoValido(TipoAnuncio.ALUGUER, 30, 10, 20, 7);
			var t = CriarTransacaoPendente(pedido);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			await Assert.ThrowsAsync<BusinessException>(() => _service.ConfirmarDevolucaoAsync(t.Id, vendedorId: 20));

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Never);
		}

		[Fact]
		public async Task ConfirmarDevolucaoAsync_ComDadosValidos_DeveConcluirAtivarAnuncioEConfirmarDevolucao_ECreditarPontos()
		{
			var anuncio = CriarAnuncioValido(TipoAnuncio.ALUGUER, id: 7);
			anuncio.AtualizarEstadoAnuncio(EstadoAnuncio.INDISPONIVEL);

			var pedido = CriarPedidoValido(TipoAnuncio.ALUGUER, 90, 10, 20, anuncio.Id);
			var t = CriarTransacaoPendente(pedido);
			t.AtualizarEstado(EstadoTransacao.DEVOLUCAO_PENDENTE);

			var devolucao = Devolucao.CriarDevolucao(t.Id, pedido.CompradorId);

			_transacaoRepoMock.Setup(x => x.ObterPorIdAsync(t.Id)).ReturnsAsync(t);
			_pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);
			_anuncioRepoMock.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
			_devolucaoRepoMock.Setup(x => x.ObterPorTransacaoIdAsync(t.Id)).ReturnsAsync(devolucao);

			_transacaoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<Transacao>())).Returns(Task.CompletedTask);
			_anuncioRepoMock.Setup(x => x.Atualizar(It.IsAny<Anuncio>())).Returns(Task.CompletedTask);
			_devolucaoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<Devolucao>())).Returns(Task.CompletedTask);

			_pontosServMock.Setup(s => s.AdicionarPontos(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<long>())).Returns(Task.CompletedTask);

			var ex = await Record.ExceptionAsync(() => _service.ConfirmarDevolucaoAsync(t.Id, vendedorId: 20));

			Assert.Null(ex);

			_transacaoRepoMock.Verify(
				x => x.AtualizarAsync(It.Is<Transacao>(tt => tt.EstadoTransacao == EstadoTransacao.CONCLUIDA)),
				Times.Once
			);
			_anuncioRepoMock.Verify(x => x.Atualizar(It.Is<Anuncio>(a => a.EstadoAnuncio == EstadoAnuncio.ATIVO)), Times.Once);
			_devolucaoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<Devolucao>()), Times.Once);

			var pontos = (int)t.ValorFinal;
			_pontosServMock.Verify(s => s.AdicionarPontos(10, pontos, t.Id), Times.Once);
			_pontosServMock.Verify(s => s.AdicionarPontos(20, pontos, t.Id), Times.Once);

			_notiServMock.Verify(s => s.CriarNotificacaoAsync(
				It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Exactly(2));
		}




		[Fact]
		public async Task ObterRegistoAsync_FiltraPorPapelComprador_DeveRetornarSoCompras()
		{
			long userId = 10;

			var a1 = CriarAnuncioValido(TipoAnuncio.VENDA, 1);
			var p1 = CriarPedidoValido(TipoAnuncio.VENDA, 100, userId, 20, a1.Id);
			var t1 = CriarTransacaoPendente(p1);

			var a2 = CriarAnuncioValido(TipoAnuncio.ALUGUER, 2);
			var p2 = CriarPedidoValido(TipoAnuncio.ALUGUER, 50, 30, userId, a2.Id);
			var t2 = CriarTransacaoPendente(p2);

			// CORREÇÃO: Adicionado o 5º argumento "comprador" para coincidir com o filtro
			_transacaoRepoMock
				.Setup(r => r.ObterDoUtilizadorAsync(userId, null, null, null))
				.ReturnsAsync(new List<Transacao> { t1, t2 });

			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(p1.Id)).ReturnsAsync(p1);
			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(p2.Id)).ReturnsAsync(p2);
			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(a1.Id)).ReturnsAsync(a1);
			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(a2.Id)).ReturnsAsync(a2);

			var filtro = new TransacaoFiltroDTO { Papel = "comprador" };
			var lista = await _service.ObterRegistoAsync(userId, filtro);

			Assert.Single(lista);
			Assert.Equal("COMPRADOR", lista[0].Papel);
			Assert.Equal(t1.Id, lista[0].Id);
		}

		[Fact]
		public async Task ObterRegistoAsync_FiltraPorPapelVendedor_DeveRetornarSoVendas()
		{
			long userId = 10;

			var a1 = CriarAnuncioValido(TipoAnuncio.VENDA, 1);
			var p1 = CriarPedidoValido(TipoAnuncio.VENDA, 100, userId, 20, a1.Id);
			var t1 = CriarTransacaoPendente(p1);

			var a2 = CriarAnuncioValido(TipoAnuncio.ALUGUER, 2);
			var p2 = CriarPedidoValido(TipoAnuncio.ALUGUER, 50, 30, userId, a2.Id);
			var t2 = CriarTransacaoPendente(p2);

			// CORREÇÃO: Adicionado o 5º argumento "VENDEDOR" para coincidir com o filtro
			_transacaoRepoMock
				.Setup(r => r.ObterDoUtilizadorAsync(userId, null, null, null))
				.ReturnsAsync(new List<Transacao> { t1, t2 });

			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(p1.Id)).ReturnsAsync(p1);
			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(p2.Id)).ReturnsAsync(p2);
			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(a1.Id)).ReturnsAsync(a1);
			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(a2.Id)).ReturnsAsync(a2);

			var filtro = new TransacaoFiltroDTO { Papel = "VENDEDOR" };
			var lista = await _service.ObterRegistoAsync(userId, filtro);

			Assert.Single(lista);
			Assert.Equal("VENDEDOR", lista[0].Papel);
			Assert.Equal(t2.Id, lista[0].Id);
		}

		[Fact]
		public async Task ObterRegistoAsync_FiltraPorEstado_DeveAplicarFiltro()
		{
			long userId = 10;

			var p = CriarPedidoValido(TipoAnuncio.VENDA, 100, userId, 20, 1);
			var t1 = CriarTransacaoPendente(p);
			var t2 = CriarTransacaoPendente(p);
			t2.AtualizarEstado(EstadoTransacao.CONCLUIDA);

			// CORREÇÃO: Adicionado null como 5º argumento (Papel)
			_transacaoRepoMock
			.Setup(r => r.ObterDoUtilizadorAsync(userId, null, null, EstadoTransacao.CONCLUIDA))
			.ReturnsAsync(new List<Transacao> { t2 });

			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(p.Id)).ReturnsAsync(p);
			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(p.AnuncioId)).ReturnsAsync(CriarAnuncioValido());

			var lista = await _service.ObterRegistoAsync(userId, new TransacaoFiltroDTO { Estado = EstadoTransacao.CONCLUIDA });

			Assert.Single(lista);
			Assert.Equal(EstadoTransacao.CONCLUIDA.ToString(), lista[0].Estado);
		}

		[Fact]
		public async Task ObterRegistoAsync_ComIntervaloDatas_DevePassarParaRepositorio()
		{
			long userId = 10;

			var de = new DateTime(2025, 1, 1);
			var ate = new DateTime(2025, 1, 31);

			var filtro = new TransacaoFiltroDTO
			{
				De = de,
				Ate = ate,
				Estado = EstadoTransacao.PENDENTE
			};

			// CORREÇÃO: Adicionado null como 5º argumento
			_transacaoRepoMock
				.Setup(r => r.ObterDoUtilizadorAsync(userId, de, ate, EstadoTransacao.PENDENTE))
				.ReturnsAsync(new List<Transacao>());

			await _service.ObterRegistoAsync(userId, filtro);

			// CORREÇÃO: Verify também precisa do 5º argumento
			_transacaoRepoMock.Verify(
				r => r.ObterDoUtilizadorAsync(userId, de, ate, EstadoTransacao.PENDENTE),
				Times.Once
			);
		}

		[Fact]
		public async Task ObterRegistoAsync_SemResultados_DeveRetornarListaVazia()
		{
			long userId = 10;

			// CORREÇÃO: Adicionado null
			_transacaoRepoMock
				.Setup(r => r.ObterDoUtilizadorAsync(userId, null, null, null))
				.ReturnsAsync(new List<Transacao>());

			var lista = await _service.ObterRegistoAsync(userId, new TransacaoFiltroDTO());

			Assert.NotNull(lista);
			Assert.Empty(lista);
		}

		[Fact]
		public async Task ObterRegistoAsync_SemFiltros_DeveMapearTituloEImagem()
		{
			long userId = 10;

			var pedido = CriarPedidoValido(TipoAnuncio.VENDA, 60, userId, 20, 7);
			var trans = CriarTransacaoPendente(pedido);

			// CORREÇÃO: Adicionado null
			_transacaoRepoMock
				.Setup(r => r.ObterDoUtilizadorAsync(userId, null, null, null))
				.ReturnsAsync(new List<Transacao> { trans });

			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

			var anuncio = CriarAnuncioValido(TipoAnuncio.VENDA, id: pedido.AnuncioId);
			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(pedido.AnuncioId)).ReturnsAsync(anuncio);

			var lista = await _service.ObterRegistoAsync(userId, new TransacaoFiltroDTO());

			Assert.NotEmpty(lista);
			var dto = lista.First(x => x.Id == trans.Id);

			var tituloEsperado = anuncio.Livro?.Titulo ?? "Título indisponível";
			Assert.Equal(tituloEsperado, dto.TituloAnuncio);

			var imagemEsperada = anuncio.Imagens?.Split(';').FirstOrDefault();
			Assert.Equal(imagemEsperada, dto.ImagemAnuncio);
		}

		[Fact]
		public async Task ObterRegistoAsync_SemFiltros_DeveEnriquecerEMapear_DuplaPerspetiva()
		{
			long userId = 10;

			// ... (resto da configuração dos dados de teste mantida igual) ...
			var anuncio1 = CriarAnuncioValido(TipoAnuncio.VENDA, 1);
			typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncio1, Livro.AdicionarLivro(111, "Livro 1", "Autor"));
			var pedido1 = CriarPedidoValido(TipoAnuncio.VENDA, 100, userId, 20, anuncio1.Id);
			var t1 = CriarTransacaoPendente(pedido1);
			typeof(Transacao).GetProperty("Id")!.SetValue(t1, 2001L);

			var anuncio2 = CriarAnuncioValido(TipoAnuncio.ALUGUER, 2);
			typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncio2, Livro.AdicionarLivro(222, "Livro 2", "Autor"));
			var pedido2 = CriarPedidoValido(TipoAnuncio.ALUGUER, 50, 30, userId, anuncio2.Id);
			var t2 = CriarTransacaoPendente(pedido2);
			typeof(Transacao).GetProperty("Id")!.SetValue(t2, 2002L);

			// CORREÇÃO: Adicionado null como 5º argumento
			_transacaoRepoMock
				.Setup(r => r.ObterDoUtilizadorAsync(userId, null, null, null))
				.ReturnsAsync(new List<Transacao> { t1, t2 });

			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(pedido1.Id)).ReturnsAsync(pedido1);
			_pedidoRepoMock.Setup(r => r.ObterPorIdAsync(pedido2.Id)).ReturnsAsync(pedido2);

			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(anuncio1.Id)).ReturnsAsync(anuncio1);
			_anuncioRepoMock.Setup(r => r.ObterPorIdAsync(anuncio2.Id)).ReturnsAsync(anuncio2);

			var lista = await _service.ObterRegistoAsync(userId, new TransacaoFiltroDTO());

			var regs = lista.GroupBy(x => x.Id).Select(g => g.First()).ToList();
			Assert.Equal(2, regs.Count);

			var r1 = regs.Single(x => x.Id == 2001);
			Assert.Equal(20, r1.OutroUtilizadorId);

			var r2 = regs.Single(x => x.Id == 2002);
			Assert.Equal(30, r2.OutroUtilizadorId);
		}
	}
}
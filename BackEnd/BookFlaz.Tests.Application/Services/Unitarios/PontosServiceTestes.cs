using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using Moq;

namespace BookFlaz.Tests.Application.Services.Unitarios
{
    public class PontosServiceTestes
    {
        private readonly Mock<IMovimentoPontosRepository> _movRepo = new();
        private readonly Mock<ITransacaoRepository> _transRepo = new();
        private readonly Mock<IPedidoTransacaoRepository> _pedidoRepo = new();
        private readonly Mock<IAnuncioRepository> _anuncioRepo = new();
        private readonly Mock<IClienteRepository> _clienteRepo = new();

        private readonly PontosService _sut;

        public PontosServiceTestes()
        {
            _sut = new PontosService(
                _movRepo.Object,
                _transRepo.Object,
                _pedidoRepo.Object,
                _anuncioRepo.Object,
                _clienteRepo.Object
            );
        }

        [Fact]
        public async Task ObterPontosAsync_QuandoClienteNaoExiste_DeveLancarNotFoundException()
        {
            _clienteRepo.Setup(r => r.ExisteAsync(It.IsAny<long>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.ObterPontosAsync(1));
        }

        [Fact]
        public async Task ObterPontosAsync_QuandoClienteExiste_DeveRetornarPontos()
        {
            _clienteRepo.Setup(r => r.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);

            _clienteRepo.Setup(r => r.ObterPontosAsync(1)).ReturnsAsync(150);

            var result = await _sut.ObterPontosAsync(1);

            Assert.Equal(150, result);
        }

        [Fact]
        public async Task ObterHistoricoAsync_QuandoClienteNaoExiste_DeveLancarNotFoundException()
        {
            _clienteRepo.Setup(r => r.ExisteAsync(It.IsAny<long>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.ObterHistoricoAsync(1));
        }

        [Fact]
        public async Task ObterHistoricoAsync_QuandoSemMovimentos_DeveLancarNotFoundException()
        {
            _clienteRepo.Setup(r => r.ExisteAsync(1)).ReturnsAsync(true);
            _movRepo.Setup(r => r.ObterMovimentosPorClienteAsync(1)).ReturnsAsync(new List<MovimentoPontos>());

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.ObterHistoricoAsync(1));
        }

        [Fact]
        public async Task ObterHistoricoAsync_QuandoMovimentosExistem_DeveRetornarListaOrdenada()
        {
            _clienteRepo.Setup(r => r.ExisteAsync(1)).ReturnsAsync(true);

            var mov1 = new MovimentoPontos(
                id: 1,
                clienteId: 1,
                transacaoId: 10,
                dataMovimento: DateTime.UtcNow.AddMinutes(-5),
                tipoMovimento: TipoMovimento.GANHO,
                quantidade: 100
            );

            var mov2 = new MovimentoPontos(
                id: 2,
                clienteId: 1,
                transacaoId: 10,
                dataMovimento: DateTime.UtcNow,
                tipoMovimento: TipoMovimento.GASTO,
                quantidade: -20
            );

            _movRepo.Setup(r => r.ObterMovimentosPorClienteAsync(1))
                .ReturnsAsync(new List<MovimentoPontos> { mov1, mov2 });

            var transacao = Transacao.CriarTransacao(
                valorTotal: 50,
                pontosUsados: 100,
                pedidoId: 5,
                compradorId: 1,
                vendedorId: 2
            );

            _transRepo.Setup(r => r.ObterPorIdAsync(10))
                .ReturnsAsync(transacao);

            var pedido = PedidoTransacao.CriarPedido(
                valor: 20,
                tipoAnuncio: TipoAnuncio.VENDA,
                anuncioId: 100,
                compradorId: 1,
                vendedorId: 2,
                destinatarioId: 2,
                remetenteId: 1,
                conversaId: 88
            );

            _pedidoRepo.Setup(r => r.ObterPorIdAsync(5))
                .ReturnsAsync(pedido);

            var anuncio = Anuncio.CriarAnuncio(
                preco: 10,
                livroIsbn: 999,
                categoriaId: 1,
                vendedorId: 2,
                estadoLivro: EstadoLivro.USADO,
                tipoAnuncio: TipoAnuncio.VENDA,
                imagens: "imagem.jpg"
            );

            _anuncioRepo.Setup(r => r.ObterPorIdAsync(100))
                .ReturnsAsync(anuncio);

            var resultado = await _sut.ObterHistoricoAsync(1);

            Assert.Equal(2, resultado.Count);
            Assert.True(resultado[0].Data > resultado[1].Data);
            Assert.Equal("imagem.jpg", resultado[0].ImagemAnuncio);
        }

        [Fact]
        public async Task AdicionarPontos_ClienteNaoExiste_DeveLancarNotFoundException()
        {
            _clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync((Cliente)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.AdicionarPontos(1, 10, 1));
        }

        [Fact]
        public async Task AdicionarPontos_TransacaoNaoExiste_DeveLancarNotFoundException()
        {
            _clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(new Cliente());

            _transRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync((Transacao)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.AdicionarPontos(1, 10, 1));
        }

        [Fact]
        public async Task AdicionarPontos_PontosInvalidos_DeveLancarValidationException()
        {
            var cliente = new Cliente();

            _clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(cliente);

            _transRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(new Transacao());

            await Assert.ThrowsAsync<ValidationException>(() =>
                _sut.AdicionarPontos(1, -5, 1));
        }

        [Fact]
        public async Task AdicionarPontos_ComSucesso_DeveAtualizarERegistrarMovimento()
        {
            var cliente = new Cliente();

            _clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(cliente);

            _transRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(new Transacao());

            await _sut.AdicionarPontos(1, 5, 1);

            _clienteRepo.Verify(x => x.AtualizarAsync(cliente), Times.Once);
            _movRepo.Verify(x => x.AdicionarMovimento(It.IsAny<MovimentoPontos>()), Times.Once);
        }

        [Fact]
        public async Task RemoverPontos_ClienteNaoExiste_DeveLancarNotFoundException()
        {
            _clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync((Cliente)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.RemoverPontos(1, 10, 1));
        }

        [Fact]
        public async Task RemoverPontos_TransacaoNaoExiste_DeveLancarNotFoundException()
        {
            _clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(new Cliente());

            _transRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync((Transacao)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.RemoverPontos(1, 10, 1));
        }

        [Fact]
        public async Task RemoverPontos_PontosInvalidos_DeveLancarValidationException()
        {
            var cliente = new Cliente();

            cliente.AdicionarPontos(5); 

            _clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(cliente);

            _transRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(new Transacao());

            await Assert.ThrowsAsync<ValidationException>(() =>
                _sut.RemoverPontos(1, 10, 1));
        }

        [Fact]
        public async Task RemoverPontos_ComSucesso_DeveAtualizarClienteERegistarMovimento()
        {
            var cliente = new Cliente();
            cliente.AdicionarPontos(20);

            _clienteRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(cliente);

            _transRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>())).ReturnsAsync(new Transacao());

            await _sut.RemoverPontos(1, 10, 1);

            _clienteRepo.Verify(x => x.AtualizarAsync(cliente), Times.Once);
            _movRepo.Verify(x => x.AdicionarMovimento(It.IsAny<MovimentoPontos>()), Times.Once);
        }
    }
}

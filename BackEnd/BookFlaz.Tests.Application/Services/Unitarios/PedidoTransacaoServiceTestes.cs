using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookFlaz.Tests.Application.Services
{
    public class PedidoTransacaoServiceTestes
    {
        private readonly Mock<IPedidoTransacaoRepository> _pedidoRepoMock;
        private readonly Mock<IConversaRepository> _conversaRepoMock;
        private readonly Mock<IAnuncioRepository> _anuncioRepoMock;
        private readonly Mock<IClienteRepository> _clienteRepoMock;
        private readonly PedidoTransacaoService _service;
        private readonly Mock<INotificacaoService> _notificacaoServ;
        private readonly Mock<IFavoritoRepository> _favoritoRepoMock;

        public PedidoTransacaoServiceTestes()
        {
            _pedidoRepoMock = new Mock<IPedidoTransacaoRepository>();
            _conversaRepoMock = new Mock<IConversaRepository>();
            _anuncioRepoMock = new Mock<IAnuncioRepository>();
            _clienteRepoMock = new Mock<IClienteRepository>();
            _notificacaoServ = new Mock<INotificacaoService>();
            _favoritoRepoMock = new Mock<IFavoritoRepository>();

            _service = new PedidoTransacaoService(
                _pedidoRepoMock.Object,
                _conversaRepoMock.Object,
                _anuncioRepoMock.Object,
                _clienteRepoMock.Object,
                _notificacaoServ.Object,
                _favoritoRepoMock.Object
            );
        }

        private Anuncio CriarAnuncioValido()
        {
            var anuncio = Anuncio.CriarAnuncio(30, 123, 1, 2, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");
            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncio, Livro.AdicionarLivro(123, "OLAOLA", "RUI"));
            typeof(Anuncio).GetProperty("Id")!.SetValue(anuncio, 1);
            return anuncio;
        }


        private CriarPedidoTransacaoDTO CriarDtoValido() => new()
        {
            ValorProposto = 30,
            AnuncioId = 1,
            ConversaId = null,
            DiasDeAluguel = null
        };

        private PedidoTransacao CriarPedidoValido()
        {
            var anuncio = CriarAnuncioValido();

            return PedidoTransacao.CriarPedido(
                valor: 30,
                tipoAnuncio: anuncio.TipoAnuncio,
                anuncioId: anuncio.Id,
                compradorId: 1,
                vendedorId: 2,
                destinatarioId: 2,
                remetenteId: 1,
                conversaId: 1,
                diasDeAluguel: null
            );
        }

        [Fact]
        public async Task CriarPedidoAsync_ComDadosValidos_DeveCriarPedidoSemErro()
        {
            var dto = CriarDtoValido();
            var anuncio = CriarAnuncioValido();

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);
            _conversaRepoMock.Setup(x => x.ObterEntreAsync(1, anuncio.VendedorId, anuncio.Id))
                .ReturnsAsync((Conversa)null!);
            _conversaRepoMock.Setup(x => x.AdicionarAsync(It.IsAny<Conversa>())).Returns(Task.CompletedTask);

            _pedidoRepoMock.Setup(x => x.AdicionarAsync(It.IsAny<PedidoTransacao>()))
                .Returns(Task.CompletedTask);

            var exception = await Record.ExceptionAsync(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Null(exception);
            _pedidoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<PedidoTransacao>()), Times.Once);
            _notificacaoServ.Verify(s => s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()), Times.Once);
        }

        [Fact]
        public async Task CriarPedidoAsync_AnuncioInexistente_DeveLancarNotFound()
        {
            var dto = CriarDtoValido();
            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync((Anuncio)null!);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("O anúncio especificado não existe.", ex.Message);
        }

        [Fact]
        public async Task CriarPedidoAsync_ClienteInexistente_DeveLancarNotFound()
        {
            var dto = CriarDtoValido();
            var anuncio = CriarAnuncioValido();

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("O utilizador especificado não existe.", ex.Message);
        }

        [Fact]
        public async Task CriarPedidoAsync_UltimoPedidoAceito_DeveLancarErro()
        {
            var dto = CriarDtoValido();
            var anuncio = CriarAnuncioValido();
            var conversa = Conversa.CriarConversa(anuncio.VendedorId, 1, anuncio.Id);

            var pedidoAceito = CriarPedidoValido();
            pedidoAceito.AlterarEstado(EstadoPedido.ACEITE);

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);
            _conversaRepoMock.Setup(x => x.ObterEntreAsync(1, anuncio.VendedorId, anuncio.Id))
                .ReturnsAsync(conversa);
            _pedidoRepoMock.Setup(x => x.ObterUltimoPedidoDaConversaAsync(conversa.Id))
                .ReturnsAsync(pedidoAceito);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("Já existe uma transação aceite nesta conversa para este anúncio. Não é possível criar um novo pedido.", ex.Message);
        }

        [Fact]
        public async Task CriarPedidoAsync_ValorZeroEmVenda_DeveLancarValidationException()
        {
            var dto = CriarDtoValido();
            dto.ValorProposto = 0;
            var anuncio = CriarAnuncioValido();

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("O valor proposto deve ser maior que zero.", ex.Message);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task CriarPedidoAsync_TipoAluguerSemDias_DeveLancarValidationException()
        {
            var dto = CriarDtoValido();
            var anuncio = Anuncio.CriarAnuncio(10, 321, 1, 2, EstadoLivro.NOVO, TipoAnuncio.ALUGUER, "livro.png");

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("Deves indicar o número de dias para o aluguel.", ex.Message);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task CriarPedidoAsync_VendedorTentaEnviarPrimeiraProposta_DeveLancarUnauthorizedActionException()
        {
            var dto = CriarDtoValido();
            var anuncio = CriarAnuncioValido();

            long vendedorId = anuncio.VendedorId;
            dto.ConversaId = null;

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(vendedorId)).ReturnsAsync(true);

            _conversaRepoMock.Setup(x => x.ObterEntreAsync(vendedorId, anuncio.VendedorId, anuncio.Id))
                .ReturnsAsync((Conversa)null!);

            var ex = await Assert.ThrowsAsync<UnauthorizedActionException>(() => _service.CriarPedidoAsync(dto, vendedorId));

            Assert.Equal("O vendedor não pode enviar uma proposta sem o comprador ter iniciado a conversa.", ex.Message);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task CriarPedidoAsync_ComPedidoPendenteAnterior_DeveCancelarAnteriorSemErro()
        {
            var dto = CriarDtoValido();
            var anuncio = CriarAnuncioValido();
            var pendente = CriarPedidoValido();

            var conversa = Conversa.CriarConversa(anuncio.VendedorId, 1, anuncio.Id);

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);
            _conversaRepoMock.Setup(x => x.ObterEntreAsync(1, anuncio.VendedorId, anuncio.Id))
                .ReturnsAsync(conversa);

            _pedidoRepoMock.Setup(x => x.ObterUltimoPedidoDaConversaAsync(conversa.Id))
                .ReturnsAsync((PedidoTransacao)null!);

            _pedidoRepoMock.Setup(x => x.ObterPendenteEntreAsync(1, anuncio.VendedorId, anuncio.Id))
                .ReturnsAsync(pendente);

            _pedidoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>())).Returns(Task.CompletedTask);
            _pedidoRepoMock.Setup(x => x.AdicionarAsync(It.IsAny<PedidoTransacao>())).Returns(Task.CompletedTask);

            var exception = await Record.ExceptionAsync(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Null(exception);

            _pedidoRepoMock.Verify(x =>
                x.AtualizarAsync(It.Is<PedidoTransacao>(p => p.EstadoPedido == EstadoPedido.CANCELADO)),
                Times.Once);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Once);
        }

        [Fact]
        public async Task CriarPedidoAsync_ValorPropostoNegativo_DeveLancarValidationException()
        {
            var dto = CriarDtoValido();
            dto.ValorProposto = -5; 
            var anuncio = CriarAnuncioValido();

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("O valor proposto deve ser maior que zero.", ex.Message);
        }

        [Fact]
        public async Task CriarPedidoAsync_UsuarioNaoPertenceAConversa_DeveLancarUnauthorizedAction()
        {
            var dto = CriarDtoValido();
            dto.ConversaId = 1;

            var anuncio = CriarAnuncioValido();

            var conversa = Conversa.CriarConversa(anuncio.VendedorId, 9, anuncio.Id); 

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);
            _conversaRepoMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync(conversa);

            var ex = await Assert.ThrowsAsync<UnauthorizedActionException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("Não podes interagir numa conversa onde não participas.", ex.Message);
        }

        [Fact]
        public async Task CriarPedidoAsync_UltimoPedidoRejeitado_DeveCriarNovo()
        {
            var dto = CriarDtoValido();
            var anuncio = CriarAnuncioValido();

            var conversa = Conversa.CriarConversa(anuncio.VendedorId, 1, anuncio.Id);
            var pedidoRejeitado = CriarPedidoValido();
            pedidoRejeitado.AlterarEstado(EstadoPedido.REJEITADO);

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);
            _conversaRepoMock.Setup(x => x.ObterEntreAsync(1, anuncio.VendedorId, anuncio.Id)).ReturnsAsync(conversa);
            _pedidoRepoMock.Setup(x => x.ObterUltimoPedidoDaConversaAsync(conversa.Id)).ReturnsAsync(pedidoRejeitado);
            _pedidoRepoMock.Setup(x => x.AdicionarAsync(It.IsAny<PedidoTransacao>())).Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Null(ex);
            _pedidoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<PedidoTransacao>()), Times.Once);
        }

        [Fact]
        public async Task CriarPedidoAsync_ValorPropostoMaiorQue10000_DeveLancarValidationException()
        {
            var dto = CriarDtoValido();
            dto.ValorProposto = 20000; 
            var anuncio = CriarAnuncioValido();

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("O valor proposto deve ser menor que 10000.", ex.Message);
        }

        [Fact]
        public async Task CriarPedidoAsync_DiasDeAluguelInvalido_DeveLancarValidationException()
        {
            var dto = CriarDtoValido();
            dto.DiasDeAluguel = -2;
            var anuncio = Anuncio.CriarAnuncio(10, 321, 1, 2, EstadoLivro.NOVO, TipoAnuncio.ALUGUER, "livro.png");

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Equal("Deves indicar o número de dias para o aluguel.", ex.Message);
        }

        [Fact]
        public async Task CriarPedidoAsync_UltimoPedidoPendente_DeveCancelarEPersistirNovo()
        {
            var dto = CriarDtoValido();
            var anuncio = CriarAnuncioValido();
            var conversa = Conversa.CriarConversa(anuncio.VendedorId, 1, anuncio.Id);

            var pedidoPendente = CriarPedidoValido(); 

            _anuncioRepoMock.Setup(x => x.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepoMock.Setup(x => x.ExisteAsync(1)).ReturnsAsync(true);

            _conversaRepoMock.Setup(x => x.ObterEntreAsync(1, anuncio.VendedorId, anuncio.Id))
                .ReturnsAsync(conversa);

            _pedidoRepoMock.Setup(x => x.ObterUltimoPedidoDaConversaAsync(conversa.Id)).ReturnsAsync((PedidoTransacao)null!);

            _pedidoRepoMock.Setup(x => x.ObterPendenteEntreAsync(1, anuncio.VendedorId, anuncio.Id)).ReturnsAsync(pedidoPendente);

            _pedidoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>())).Returns(Task.CompletedTask);

            _pedidoRepoMock.Setup(x => x.AdicionarAsync(It.IsAny<PedidoTransacao>())).Returns(Task.CompletedTask);

            var ex = await Record.ExceptionAsync(() => _service.CriarPedidoAsync(dto, 1));

            Assert.Null(ex);

            _pedidoRepoMock.Verify(x => x.AtualizarAsync(It.Is<PedidoTransacao>(p => p.EstadoPedido == EstadoPedido.CANCELADO)), Times.Once);

            _pedidoRepoMock.Verify(x => x.AdicionarAsync(It.IsAny<PedidoTransacao>()), Times.Once);

            _notificacaoServ.Verify(x => x.CriarNotificacaoAsync(It.IsAny<string>(), TipoNotificacao.TRANSACAO, It.IsAny<long>()), Times.Once);
        }


        [Fact]
        public async Task AceitarPedidoAsync_PedidoNaoEncontrado_DeveLancarInvalidOperationException()
        {
            _pedidoRepoMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync((PedidoTransacao)null!);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.AceitarPedidoAsync(1, 1));

            Assert.Equal("Pedido não encontrado.", ex.Message);
        }

        [Fact]
        public async Task AceitarPedidoAsync_ComPedidoValido_DeveExecutarSemErro()
        {
            var pedido = CriarPedidoValido();

            _pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);
            _pedidoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>())).Returns(Task.CompletedTask);

            var fav1 = Favorito.AdicionarFavorito(clienteId: 10, anuncioId: pedido.AnuncioId);
            var fav2 = Favorito.AdicionarFavorito(clienteId: 11, anuncioId: pedido.AnuncioId);

            _favoritoRepoMock.Setup(x => x.ObterPorAnuncioAsync(pedido.AnuncioId))
                             .ReturnsAsync(new List<Favorito> { fav1, fav2 });

            var ex = await Record.ExceptionAsync(() => _service.AceitarPedidoAsync(pedido.Id, pedido.DestinatarioId));

            Assert.Null(ex);

            _pedidoRepoMock.Verify(x => x.AtualizarAsync(It.Is<PedidoTransacao>(p => p.EstadoPedido == EstadoPedido.ACEITE)), Times.Once);
            _notificacaoServ.Verify(s => s.CriarNotificacaoAsync(
                It.IsAny<string>(), TipoNotificacao.FAVORITO, 10), Times.Once);

            _notificacaoServ.Verify(s => s.CriarNotificacaoAsync(
                It.IsAny<string>(), TipoNotificacao.FAVORITO, 11), Times.Once);
        }

        [Fact]
        public async Task AceitarPedidoAsync_ComEstadoJaProcessado_DeveLancarInvalidOperationException()
        {
            var pedido = CriarPedidoValido();

            pedido.AlterarEstado(EstadoPedido.ACEITE);

            _pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

            var fav1 = Favorito.AdicionarFavorito(clienteId: 10, anuncioId: pedido.AnuncioId);
            var fav2 = Favorito.AdicionarFavorito(clienteId: 11, anuncioId: pedido.AnuncioId);

            _favoritoRepoMock.Setup(x => x.ObterPorAnuncioAsync(pedido.AnuncioId))
                             .ReturnsAsync(new List<Favorito> { fav1, fav2 });

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.AceitarPedidoAsync(pedido.Id, pedido.DestinatarioId));

            Assert.Equal("O pedido já foi processado.", ex.Message);
            _pedidoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>()), Times.Never);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task AceitarPedidoAsync_QuandoUtilizadorNaoEhDestinatario_DeveLancarInvalidOperationException()
        {
            var pedido = CriarPedidoValido();

            long utilizadorErrado = 99;

            _pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

            var fav1 = Favorito.AdicionarFavorito(clienteId: 10, anuncioId: pedido.AnuncioId);
            var fav2 = Favorito.AdicionarFavorito(clienteId: 11, anuncioId: pedido.AnuncioId);

            _favoritoRepoMock.Setup(x => x.ObterPorAnuncioAsync(pedido.AnuncioId))
                             .ReturnsAsync(new List<Favorito> { fav1, fav2 });

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>_service.AceitarPedidoAsync(pedido.Id, utilizadorErrado));

            Assert.Equal("Apenas o destinatário pode aceitar o pedido.", ex.Message);

            _pedidoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>()), Times.Never);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task RejeitarPedidoAsync_PedidoNaoEncontrado_DeveLancarInvalidOperationException()
        {
            _pedidoRepoMock.Setup(x => x.ObterPorIdAsync(1)).ReturnsAsync((PedidoTransacao)null!);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.RejeitarPedidoAsync(1, 1));

            Assert.Equal("Pedido não encontrado.", ex.Message);
            _pedidoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>()), Times.Never);
        }

        [Fact]
        public async Task RejeitarPedidoAsync_ComPedidoValido_DeveExecutarSemErro()
        {
            var pedido = CriarPedidoValido();

            _pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);
            _pedidoRepoMock.Setup(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>())).Returns(Task.CompletedTask);

            var exception = await Record.ExceptionAsync(() => _service.RejeitarPedidoAsync(pedido.Id, pedido.DestinatarioId));

            Assert.Null(exception);

            _pedidoRepoMock.Verify(x => x.AtualizarAsync(It.Is<PedidoTransacao>(p => p.EstadoPedido == EstadoPedido.REJEITADO)), Times.Once);
        }

        [Fact]
        public async Task RejeitarPedidoAsync_ComEstadoJaProcessado_DeveLancarInvalidOperationException()
        {
            var pedido = CriarPedidoValido();

            pedido.AlterarEstado(EstadoPedido.ACEITE);

            _pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.RejeitarPedidoAsync(pedido.Id, pedido.DestinatarioId));

            Assert.Equal("O pedido já foi processado.", ex.Message);

            _pedidoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>()), Times.Never);
        }

        [Fact]
        public async Task RejeitarPedidoAsync_QuandoUtilizadorNaoEhDestinatario_DeveLancarInvalidOperationException()
        {
            var pedido = CriarPedidoValido();

            long utilizadorErrado = 99;

            _pedidoRepoMock.Setup(x => x.ObterPorIdAsync(pedido.Id)).ReturnsAsync(pedido);

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _service.RejeitarPedidoAsync(pedido.Id, utilizadorErrado));

            Assert.Equal("Apenas o destinatário pode aceitar o pedido.", ex.Message);

            _pedidoRepoMock.Verify(x => x.AtualizarAsync(It.IsAny<PedidoTransacao>()), Times.Never);
        }
    }
}

// BookFlaz.Tests/Application/Services/Unitarios/ChatServiceTestes.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using Moq;

namespace BookFlaz.Tests.Application.Services.Unitarios
{
    public class ChatServiceTestes
    {
        private readonly Mock<IChatRepository> _chatRepo = new();
        private readonly Mock<IAnuncioRepository> _anuncioRepo = new();
        private readonly Mock<IClienteRepository> _clienteRepo = new();
        private readonly Mock<INotificacaoService> _notificacaoServ = new();
        private readonly Mock<INotificacaoRepository> _notificacaoRepo = new();

        private readonly ChatService _sut;

        public ChatServiceTestes()
        {
            _sut = new ChatService(_chatRepo.Object, _anuncioRepo.Object, _clienteRepo.Object, _notificacaoServ.Object);
        }

        private static Anuncio UmAnuncio(long id = 10, long vendedorId = 7, long isbn = 1234567890123)
        {
            var a = Anuncio.CriarAnuncio(25, isbn, categoriaId: 1, vendedorId: vendedorId,
                                         EstadoLivro.NOVO, TipoAnuncio.VENDA, "img.jpg");
            typeof(Anuncio).GetProperty("Id")!.SetValue(a, id);
            return a;
        }

        [Fact]
        public async Task EnviarMensagem_ComDadosValidosSemConversa_PreparaConversaECriaMensagem()
        {
            var clientId = 20;
            var anuncio = UmAnuncio(id: 100, vendedorId: 50);
            var dto = new CriarMensagemDTO {AnuncioId = 100, Conteudo = "Olá" };

            _anuncioRepo.Setup(r => r.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(r => r.ExisteAsync(clientId)).ReturnsAsync(true);
            _chatRepo.Setup(r => r.ObterConversaUsandoDadosAsync(clientId, anuncio.VendedorId, anuncio.Id))
                     .ReturnsAsync((Conversa?)null);

            var conversaCriada = Conversa.CriarConversa(anuncio.VendedorId, clientId, anuncio.Id);
            typeof(Conversa).GetProperty("Id")!.SetValue(conversaCriada, 777);

            _chatRepo.Setup(r => r.CriarConversaAsync(anuncio.VendedorId, clientId, anuncio.Id))
                     .ReturnsAsync(conversaCriada);

            var mensagemCriada = Mensagem.CriarMensagem(clientId, 777, dto.Conteudo);
            _chatRepo.Setup(r => r.CriarMensagemAsync(clientId, 777, dto.Conteudo))
                     .ReturnsAsync(mensagemCriada);

            var result = await _sut.EnviarMensagem(dto, clientId);

            Assert.Equal(dto.Conteudo, result.Conteudo);
            Assert.Equal(777, result.ConversaId);

            _chatRepo.Verify(r => r.CriarConversaAsync(anuncio.VendedorId, clientId, anuncio.Id), Times.Once);
            _chatRepo.Verify(r => r.CriarMensagemAsync(clientId, 777, dto.Conteudo), Times.Once);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(
                    It.Is<string>(m => m.Contains(clientId.ToString())),
                    TipoNotificacao.CHAT,
                    anuncio.VendedorId),
                Times.Once);
        }

        [Fact]
        public async Task EnviarMensagem_ComConversaExistente_ReutilizaConversa()
        {
            var clientId = 20;
            var anuncio = UmAnuncio(id: 101, vendedorId: 60);
            var dto = new CriarMensagemDTO {AnuncioId = 101, Conteudo = "Ping" };

            _anuncioRepo.Setup(r => r.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(r => r.ExisteAsync(clientId)).ReturnsAsync(true);

            var conversa = Conversa.CriarConversa(anuncio.VendedorId, clientId, anuncio.Id);
            typeof(Conversa).GetProperty("Id")!.SetValue(conversa, 888);

            _chatRepo.Setup(r => r.ObterConversaUsandoDadosAsync(clientId, anuncio.VendedorId, anuncio.Id))
                     .ReturnsAsync(conversa);

            var mensagem = Mensagem.CriarMensagem(clientId, 888, dto.Conteudo);
            _chatRepo.Setup(r => r.CriarMensagemAsync(clientId, 888, dto.Conteudo))
                     .ReturnsAsync(mensagem);

            var result = await _sut.EnviarMensagem(dto, clientId);

            Assert.Equal(888, result.ConversaId);
            _chatRepo.Verify(r => r.CriarConversaAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            _chatRepo.Verify(r => r.CriarMensagemAsync(clientId, 888, dto.Conteudo), Times.Once);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(
                    It.Is<string>(m => m.Contains(clientId.ToString())),
                    TipoNotificacao.CHAT,
                    anuncio.VendedorId),
                Times.Once);
        }

        [Fact]
        public async Task EnviarMensagem_AnuncioNaoEncontrado_LancaExcecao()
        {
            var clientId = 20;
            var dto = new CriarMensagemDTO {AnuncioId = 999, Conteudo = "Oi" };

            _anuncioRepo.Setup(r => r.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync((Anuncio?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.EnviarMensagem(dto, clientId));
            Assert.Equal("Anúncio não encontrado.", ex.Message);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task EnviarMensagem_ClienteNaoExiste_LancaExcecao()
        {
            var clientId = 999;
            var anuncio = UmAnuncio(id: 200, vendedorId: 40);
            var dto = new CriarMensagemDTO {AnuncioId = 200, Conteudo = "Olá" };

            _anuncioRepo.Setup(r => r.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(r => r.ExisteAsync(clientId)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.EnviarMensagem(dto, clientId));
            Assert.Equal("Cliente não encontrado.", ex.Message);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task EnviarMensagem_VendedorTentandoFalarConsigoMesmo_LancaExcecao()
        {
            var clientId = 123;
            var anuncio = UmAnuncio(id: 300, vendedorId: 123);
            var dto = new CriarMensagemDTO  {AnuncioId = 300, Conteudo = "..." };

            _anuncioRepo.Setup(r => r.ObterPorIdAsync(dto.AnuncioId)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(r => r.ExisteAsync(clientId)).ReturnsAsync(true);
            _chatRepo.Setup(r => r.ObterConversaUsandoDadosAsync(clientId, anuncio.VendedorId, anuncio.Id))
                     .ReturnsAsync((Conversa?)null);

            var ex = await Assert.ThrowsAsync<UnauthorizedActionException>(() => _sut.EnviarMensagem(dto, clientId));
            Assert.Equal("Anunciante tentou iniciar conversa consigo mesmo.", ex.Message);

            _chatRepo.Verify(r => r.CriarConversaAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never);
            _chatRepo.Verify(r => r.CriarMensagemAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never);

            _notificacaoServ.Verify(s =>
                s.CriarNotificacaoAsync(It.IsAny<string>(), It.IsAny<TipoNotificacao>(), It.IsAny<long>()),
                Times.Never);
        }

        [Fact]
        public async Task ObterMensagensPorConversa_ComConversaExistente_DeveRetornarDtoComMensagensOrdenadas()
        {
            var conversaId = 777L;

            _chatRepo.Setup(r => r.ConversaExisteAsync(conversaId))
                 .ReturnsAsync(true);

            var m1 = Mensagem.CriarMensagem(10, conversaId, "A");
            await Task.Delay(2);
            var m2 = Mensagem.CriarMensagem(11, conversaId, "B");

            _chatRepo.Setup(r => r.ObterMensagensPorConversaAsync(conversaId))
                 .ReturnsAsync(new List<Mensagem> { m1, m2 });

            var pedidos = new List<PedidoTransacao>();
            _chatRepo.Setup(r => r.ObterPedidosNaConversaAsync(conversaId))
                 .ReturnsAsync(pedidos);

            var dto = await _sut.ObterMensagensPorConversa(conversaId);

            Assert.NotNull(dto);
            Assert.NotNull(dto.Mensagens);
            Assert.NotNull(dto.Pedidos);

            Assert.Equal(2, dto.Mensagens.Count);
            Assert.Equal("A", dto.Mensagens[0].Conteudo);
            Assert.Equal("B", dto.Mensagens[1].Conteudo);
            Assert.Empty(dto.Pedidos);

            _chatRepo.Verify(r => r.ConversaExisteAsync(conversaId), Times.Once);
            _chatRepo.Verify(r => r.ObterMensagensPorConversaAsync(conversaId), Times.Once);
            _chatRepo.Verify(r => r.ObterPedidosNaConversaAsync(conversaId), Times.Once);
        }


        [Fact]
        public async Task ObterMensagensPorConversa_ConversaInexistente_LancaExcecao()
        {
            _chatRepo.Setup(r => r.ConversaExisteAsync(999)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.ObterMensagensPorConversa(999));
            Assert.Equal("Conversa não encontrada.", ex.Message);
        }

        [Fact]
        public async Task ObterConversasPorUsuario_UsuarioExiste_DeveRetornarConversas()
        {
            var userId = 42L;

            _clienteRepo.Setup(r => r.ExisteAsync(userId)).ReturnsAsync(true);

            var c1 = Conversa.CriarConversa(7, userId, 1);
            var c2 = Conversa.CriarConversa(userId, 8, 2);

            _chatRepo.Setup(r => r.ObterConversasPorUsuarioAsync(userId))
                     .ReturnsAsync(new List<Conversa> { c1, c2 });

            var conversas = await _sut.ObterConversasPorUsuario(userId);

            Assert.Equal(2, conversas.Count);
        }

        [Fact]
        public async Task ObterConversasPorUsuario_UsuarioInexistente_LancaExcecao()
        {
            _clienteRepo.Setup(r => r.ExisteAsync(999)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.ObterConversasPorUsuario(999));
            Assert.Equal("Usuário não encontrado.", ex.Message);
        }
    }
}

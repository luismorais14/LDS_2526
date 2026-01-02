using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Tests.Application.Services.Unitarios
{
    public class NotificacaoServiceTestes
    {
        private readonly Mock<INotificacaoRepository> _repo = new();
        private readonly NotificacaoService _sut;

        public NotificacaoServiceTestes()
        {
            _sut = new NotificacaoService(_repo.Object);
        }

        [Fact]
        public async Task CriarNotificacaoAsync_DadosValidos_DeveCriarEDevolver()
        {
            var conteudo = "Nova mensagem";
            var tipo = TipoNotificacao.CHAT;
            var clientId = 10L;

            _repo.Setup(r => r.CriarNotificacao(It.IsAny<Notificacao>()))
                 .Returns(Task.CompletedTask);

            var n = await _sut.CriarNotificacaoAsync(conteudo, tipo, clientId);

            Assert.Equal(conteudo, n.Conteudo);
            Assert.Equal(tipo, n.TipoNotificacao);
            Assert.Equal(clientId, n.ClientId);
            _repo.Verify(r => r.CriarNotificacao(It.IsAny<Notificacao>()), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CriarNotificacaoAsync_ConteudoVazioOuNulo_DeveLancarArgumentException(string conteudo)
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CriarNotificacaoAsync(conteudo, TipoNotificacao.SISTEMA, 1L)
            );

            Assert.Contains("Conteúdo", ex.Message, StringComparison.OrdinalIgnoreCase);
            _repo.Verify(r => r.CriarNotificacao(It.IsAny<Notificacao>()), Times.Never);
        }

        [Fact]
        public async Task CriarNotificacaoAsync_ConteudoMaiorQue300_DeveLancarArgumentException()
        {
            var longo = new string('x', 301);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.CriarNotificacaoAsync(longo, TipoNotificacao.SISTEMA, 1L)
            );

            Assert.Contains("300", ex.Message);
            _repo.Verify(r => r.CriarNotificacao(It.IsAny<Notificacao>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task CriarNotificacaoAsync_ClientIdInvalido_DeveLancarArgumentOutOfRangeException(long clientId)
        {
            var ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _sut.CriarNotificacaoAsync("ok", TipoNotificacao.SISTEMA, clientId)
            );

            Assert.Contains("ClientId", ex.Message, StringComparison.OrdinalIgnoreCase);
            _repo.Verify(r => r.CriarNotificacao(It.IsAny<Notificacao>()), Times.Never);
        }

        [Fact]
        public async Task CriarNotificacaoAsync_RepositorioLanca_DeveEncapsularEmApplicationException()
        {
            _repo.Setup(r => r.CriarNotificacao(It.IsAny<Notificacao>()))
                 .ThrowsAsync(new Exception("DB down"));

            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                _sut.CriarNotificacaoAsync("ok", TipoNotificacao.SISTEMA, 1L)
            );

            Assert.Contains("Erro inesperado ao criar notificação", ex.Message);
        }

        [Fact]
        public async Task ObterNotificacoesAsync_IdValido_DeveRetornarOrdenadoPorDataDesc()
        {
            var n1 = Notificacao.CriarNotificacao("A", TipoNotificacao.SISTEMA, 1L);
            var n2 = Notificacao.CriarNotificacao("B", TipoNotificacao.CHAT, 1L);

            typeof(Notificacao).GetProperty("DataEnvio")!.SetValue(n1, DateTime.UtcNow.AddMinutes(-10));
            typeof(Notificacao).GetProperty("DataEnvio")!.SetValue(n2, DateTime.UtcNow);

            _repo.Setup(r => r.ObterNotificacoes(1L))
                 .ReturnsAsync(new List<Notificacao> { n1, n2 });

            var lista = await _sut.ObterNotificacoesAsync(1L);

            Assert.Equal(2, lista.Count);
            Assert.Equal("B", lista[0].Conteudo);
            Assert.Equal("A", lista[1].Conteudo);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ObterNotificacoesAsync_IdInvalido_DeveLancarArgumentOutOfRange(long id)
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _sut.ObterNotificacoesAsync(id)
            );
        }

        [Fact]
        public async Task ObterNotificacoesAsync_RepositorioLanca_DeveEncapsularEmApplicationException()
        {
            _repo.Setup(r => r.ObterNotificacoes(1L))
                 .ThrowsAsync(new Exception("any"));

            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                _sut.ObterNotificacoesAsync(1L)
            );

            Assert.Contains("Erro ao obter notificações", ex.Message);
        }
    }
}

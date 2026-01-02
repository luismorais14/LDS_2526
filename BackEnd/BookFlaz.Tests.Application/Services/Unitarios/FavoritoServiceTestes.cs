using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
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
    public class FavoritoServiceTestes
    {
        private readonly Mock<IFavoritoRepository> _favoritoRepo = new();
        private readonly Mock<IClienteRepository> _clienteRepo = new();
        private readonly Mock<IAnuncioRepository> _anuncioRepo = new();

        private readonly FavoritoService _sut;

        public FavoritoServiceTestes()
        {
            _sut = new FavoritoService(
                _favoritoRepo.Object,
                _clienteRepo.Object,
                _anuncioRepo.Object
            );
        }

        [Fact]
        public async Task AlternarFavoritoAsync_ClienteNaoExiste_DeveLancarExcecao()
        {
            long clienteId = 99;
            long anuncioId = 1;

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.AlternarFavoritoAsync(clienteId, anuncioId));

            Assert.Equal("Cliente não encontrado.", ex.Message);
        }

        [Fact]
        public async Task AlternarFavoritoAsync_AnuncioNaoExiste_DeveLancarExcecao()
        {
            long clienteId = 1;
            long anuncioId = 99;

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncioId))
                .ReturnsAsync((Anuncio?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.AlternarFavoritoAsync(clienteId, anuncioId));

            Assert.Equal("Anúncio não encontrado.", ex.Message);
        }

        [Fact]
        public async Task AlternarFavoritoAsync_QuandoAnuncioDoProprioCliente_DeveLancarBusinessException()
        {
            long clienteId = 1;
            long anuncioId = 10;

            var anuncio = new Anuncio();
            typeof(Anuncio).GetProperty("VendedorId")!.SetValue(anuncio, clienteId);
            typeof(Anuncio).GetProperty("EstadoAnuncio")!.SetValue(anuncio, EstadoAnuncio.ATIVO);

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncioId)).ReturnsAsync(anuncio);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.AlternarFavoritoAsync(clienteId, anuncioId));

            Assert.Equal("Não podes dar favorito ao teu próprio anúncio.", ex.Message);
        }

        [Fact]
        public async Task AlternarFavoritoAsync_QuandoAnuncioNaoAtivo_DeveLancarBusinessException()
        {
            long clienteId = 1;
            long anuncioId = 11;

            var anuncio = new Anuncio();
            typeof(Anuncio).GetProperty("VendedorId")!.SetValue(anuncio, 2L);
            typeof(Anuncio).GetProperty("EstadoAnuncio")!.SetValue(anuncio, EstadoAnuncio.VENDIDO);

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncioId)).ReturnsAsync(anuncio);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.AlternarFavoritoAsync(clienteId, anuncioId));

            Assert.Equal("O anúncio tem de estar disponível.", ex.Message);
        }

        [Fact]
        public async Task AlternarFavoritoAsync_SeJaExistir_DeveRemover()
        {
            long clienteId = 1;
            long anuncioId = 10;

            var favoritoExistente = Favorito.AdicionarFavorito(anuncioId, clienteId);

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncioId))
                .ReturnsAsync(new Anuncio());
            _favoritoRepo.Setup(x => x.ObterAsync(clienteId, anuncioId))
                .ReturnsAsync(favoritoExistente);

            await _sut.AlternarFavoritoAsync(clienteId, anuncioId);

            _favoritoRepo.Verify(x =>
                x.RemoverAsync(It.Is<Favorito>(f => f.ClienteId == clienteId && f.AnuncioId == anuncioId)),
                Times.Once);
        }

        [Fact]
        public async Task AlternarFavoritoAsync_QuandoJaTem10Favoritos_DeveRetornarErro()
        {
            long clienteId = 1;
            long anuncioId = 20;

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncioId))
                .ReturnsAsync(new Anuncio());
            _favoritoRepo.Setup(x => x.ObterAsync(clienteId, anuncioId))
                .ReturnsAsync((Favorito?)null);
            _favoritoRepo.Setup(x => x.ContarPorClienteAsync(clienteId))
                .ReturnsAsync(10);

            var ex = await Assert.ThrowsAsync<ValidationException>(() =>
                _sut.AlternarFavoritoAsync(clienteId, anuncioId));

            Assert.Equal("Já atingiste o limite máximo de 10 favoritos.", ex.Message);
        }

        [Fact]
        public async Task AlternarFavoritoAsync_AdicionarNovo_DeveRetornarSucesso()
        {
            long clienteId = 1;
            long anuncioId = 2;

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncioId))
                .ReturnsAsync(new Anuncio());
            _favoritoRepo.Setup(x => x.ObterAsync(clienteId, anuncioId))
                .ReturnsAsync((Favorito?)null);
            _favoritoRepo.Setup(x => x.ContarPorClienteAsync(clienteId)).ReturnsAsync(5);

            await _sut.AlternarFavoritoAsync(clienteId, anuncioId);

            _favoritoRepo.Verify(x =>
                x.AdicionarAsync(It.Is<Favorito>(f => f.ClienteId == clienteId && f.AnuncioId == anuncioId)),
                Times.Once);
        }

        [Fact]
        public async Task ObterAnunciosFavoritosAsync_ClienteNaoExiste_DeveLancarExcecao()
        {
            _clienteRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.ObterAnunciosFavoritosAsync(99));
            Assert.Equal("Cliente não encontrado.", ex.Message);
        }

        [Fact]
        public async Task ObterAnunciosFavoritosAsync_SemFavoritos_DeveRetornarListaVazia()
        {
            var clienteId = 1L;

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _favoritoRepo.Setup(x => x.ObterPorClienteAsync(clienteId)).ReturnsAsync(new List<Favorito>());

            var result = await _sut.ObterAnunciosFavoritosAsync(clienteId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObterAnunciosFavoritosAsync_ClienteComZeroFavoritos_DeveRetornarListaVazia()
        {
            var clienteId = 1L;

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _favoritoRepo.Setup(x => x.ObterPorClienteAsync(clienteId)).ReturnsAsync(new List<Favorito>());

            var result = await _sut.ObterAnunciosFavoritosAsync(clienteId);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObterAnunciosFavoritosAsync_DeveManterOrdemOriginalDoRepositorio()
        {
            var clienteId = 1L;

            var anuncioA = Anuncio.CriarAnuncio(20, 123, 1, 10, EstadoLivro.USADO, TipoAnuncio.VENDA, "a.jpg");
            var anuncioB = Anuncio.CriarAnuncio(30, 456, 1, 11, EstadoLivro.NOVO, TipoAnuncio.VENDA, "b.jpg");

            typeof(Anuncio).GetProperty("Id")!.SetValue(anuncioA, 5L);
            typeof(Anuncio).GetProperty("Id")!.SetValue(anuncioB, 2L);

            var favoritos = new List<Favorito>
            {
                Favorito.AdicionarFavorito(5, clienteId),
                Favorito.AdicionarFavorito(2, clienteId)
            };

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _favoritoRepo.Setup(x => x.ObterPorClienteAsync(clienteId)).ReturnsAsync(favoritos);

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync())
                .ReturnsAsync(new List<Anuncio> { anuncioA, anuncioB });

            _favoritoRepo.Setup(x => x.ContarPorAnuncioAsync(It.IsAny<long>())).ReturnsAsync(1);

            var result = await _sut.ObterAnunciosFavoritosAsync(clienteId);

            Assert.Equal(2, result.Count);
            Assert.Equal(5, result[0].Id); 
            Assert.Equal(2, result[1].Id); 
        }

        [Fact]
        public async Task ObterAnunciosFavoritosAsync_ComDadosValidos_DeveRetornarCorretamente()
        {
            var clienteId = 1L;

            var anuncio1 = Anuncio.CriarAnuncio(25, 123, 1, clienteId, EstadoLivro.USADO, TipoAnuncio.VENDA, "img1.jpg");
            var anuncio2 = Anuncio.CriarAnuncio(0, 456, 1, clienteId, EstadoLivro.NOVO, TipoAnuncio.DOACAO, "img2.jpg");

            typeof(Anuncio).GetProperty("Id")!.SetValue(anuncio1, 1L);
            typeof(Anuncio).GetProperty("Id")!.SetValue(anuncio2, 2L);

            var livro1 = new Livro(123, "Harry Potter", "J.K. Rowling");
            var livro2 = new Livro(456, "Senhor dos Anéis", "Tolkien");

            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncio1, livro1);
            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncio2, livro2);

            _clienteRepo.Setup(x => x.ExisteAsync(clienteId)).ReturnsAsync(true);
            _favoritoRepo.Setup(x => x.ObterPorClienteAsync(clienteId)).ReturnsAsync(new List<Favorito>
            {
                Favorito.AdicionarFavorito(1, clienteId),
                Favorito.AdicionarFavorito(2, clienteId)
            });

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync())
                .ReturnsAsync(new List<Anuncio> { anuncio1, anuncio2 });

            _favoritoRepo.Setup(x => x.ContarPorAnuncioAsync(It.IsAny<long>())).ReturnsAsync(5);

            var result = await _sut.ObterAnunciosFavoritosAsync(clienteId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var primeiro = result.First();
            Assert.Equal("Harry Potter", primeiro.Titulo);
            Assert.True(primeiro.Favorito);
            Assert.Equal(5, primeiro.TotalFavoritos);
            Assert.NotNull(primeiro.Imagem);
        }
    }
}

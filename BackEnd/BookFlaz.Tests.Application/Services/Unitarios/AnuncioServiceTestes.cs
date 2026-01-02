using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Exceptions;
using BookFlaz.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Tests.Application.Services.Unitarios
{
    public class AnuncioServiceTestes
    {
        private const long VENDEDOR_ID = 1;
        private const long OUTRO_USER = 99;
        private const long ADMIN_ID = 7;

        private readonly Mock<IAnuncioRepository> _anuncioRepo = new();
        private readonly Mock<ILivroRepository> _livroRepo = new();
        private readonly Mock<INotificacaoRepository> _notificacaoRepo = new();
        private readonly Mock<ICategoriaRepository> _categoriaRepo = new();
        private readonly Mock<IClienteRepository> _clienteRepo = new();
        private readonly Mock<IPedidoTransacaoRepository> _pedidoRepo = new();
        private readonly Mock<ITransacaoRepository> _transacaoRepo = new();
        private readonly Mock<IUploadService> _uploadService = new();
        private readonly Mock<IBookInfoService> _bookInfoService = new();
        private readonly Mock<IImagemService> _imagemService = new();
        private readonly Mock<INotificacaoService> _notificacaoService = new();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();

        private readonly AnuncioService _sut;

        public AnuncioServiceTestes()
        {
            _sut = new AnuncioService(
                _anuncioRepo.Object,
                _livroRepo.Object,
                _categoriaRepo.Object,
                _clienteRepo.Object,
                _pedidoRepo.Object,
                _transacaoRepo.Object,
                _uploadService.Object,
                _bookInfoService.Object,
                _imagemService.Object,
                _notificacaoRepo.Object,
                _notificacaoService.Object,
                _httpContextAccessor.Object
            );
        }

        [Fact]
        public async Task CriarAnuncioAsync_ClienteNaoExiste_DeveLancarNotFoundException()
        {
            var dto = CriarDtoValido();

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Equal($"O cliente {VENDEDOR_ID} não existe.", ex.Message);
        }


        [Fact]
        public async Task CriarAnuncioAsync_LivroNaoExiste_DeveLancarNotFoundException()
        {
            var dto = CriarDtoValido();

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(false);
            _bookInfoService.Setup(x => x.ObterLivroPorISBNAsync(It.IsAny<string>())).ReturnsAsync((LivroDTO?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Equal("O livro especificado não existe.", ex.Message);
        }

        [Fact]
        public async Task CriarAnuncioAsync_TipoAnuncioInvalido_DeveLancarDomainException()
        {
            var dto = CriarDtoValido();
            dto.TipoAnuncio = (TipoAnuncio)999;

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);

            await Assert.ThrowsAnyAsync<Exception>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));
        }

        [Fact]
        public async Task CriarAnuncioAsync_EstadoLivroInvalido_DeveLancarDomainException()
        {
            var dto = CriarDtoValido();
            dto.EstadoLivro = (EstadoLivro)999;

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);

            await Assert.ThrowsAnyAsync<Exception>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));
        }

        [Fact]
        public async Task CriarAnuncioAsync_DoacaoSemPreco_DeveCriarComSucesso()
        {
            var dto = CriarDtoValido();
            dto.TipoAnuncio = TipoAnuncio.DOACAO;
            dto.Preco = null;

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);
            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>())).ReturnsAsync(true);
            _uploadService.Setup(x => x.UploadAsync(It.IsAny<IFormFile>())).ReturnsAsync("foto.jpg");

            await _sut.CriarAnuncioAsync(dto, VENDEDOR_ID);

            _anuncioRepo.Verify(x => x.AdicionarAsync(It.IsAny<Anuncio>()), Times.Once);
        }

        [Fact]
        public async Task CriarAnuncioAsync_CategoriaNaoExiste_DeveLancarNotFoundException()
        {
            var dto = CriarDtoValido();

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Equal("A categoria especificada não existe.", ex.Message);
        }

        [Fact]
        public async Task CriarAnuncioAsync_PrecoNegativo_DeveLancarBusinessException()
        {
            var dto = CriarDtoValido();
            dto.Preco = -10;

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);
            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>()))
              .ReturnsAsync(true);
            _uploadService.Setup(x => x.UploadAsync(It.IsAny<IFormFile>()))
              .ReturnsAsync("foto.jpg");


            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Equal("Não pode ter preços negativos.", ex.Message);
        }

        [Fact]
        public async Task CriarAnuncioAsync_PrecoMaiorQue10000_DeveLancarBusinessException()
        {
            var dto = CriarDtoValido();
            dto.Preco = 15000;

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);

            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>()))
              .ReturnsAsync(true);
            _uploadService.Setup(x => x.UploadAsync(It.IsAny<IFormFile>()))
              .ReturnsAsync("foto.jpg");

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Equal("O preço da venda não pode ser superior a 10000.", ex.Message);
        }

        [Fact]
        public async Task CriarAnuncioAsync_PrecoObrigatorioParaTipoNaoDoacao_DeveLancarBusinessException()
        {
            var dto = new CriarAnuncioDTO
            {
                Preco = null,
                LivroIsbn = 123,
                CategoriaId = 1,
                EstadoLivro = EstadoLivro.USADO,
                TipoAnuncio = TipoAnuncio.VENDA,
                Imagens = new List<IFormFile> { CriarImagemFake("foto.jpg") }
            };

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Equal("O preço é obrigatório para este tipo de anúncio.", ex.Message);
        }

        [Fact]
        public async Task CriarAnuncioAsync_SemImagens_DeveLancarBusinessException()
        {
            var dto = CriarDtoValido(new List<IFormFile>());

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Equal("O anúncio deve conter pelo menos uma imagem.", ex.Message);
        }

        [Fact]
        public async Task CriarAnuncioAsync_Com5Imagens_DeveCriarComSucesso()
        {
            var imagens = Enumerable.Range(0, 5)
                .Select(i => CriarImagemFake($"img{i}.jpg"))
                .ToList();

            var dto = CriarDtoValido(imagens);

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);
            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>())).ReturnsAsync(true);
            _uploadService.Setup(x => x.UploadAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync((IFormFile f) => f.FileName);

            await _sut.CriarAnuncioAsync(dto, VENDEDOR_ID);

            _anuncioRepo.Verify(x => x.AdicionarAsync(It.IsAny<Anuncio>()), Times.Once);
        }

        [Fact]
        public async Task CriarAnuncioAsync_ComImagemExtensaoInvalidaGif_DeveLancarBusinessException()
        {
            var dto = CriarDtoValido(new List<IFormFile> { CriarImagemFake("foto.gif") });

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);

            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Contains("não parece conter um livro", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CriarAnuncioAsync_ComMaisDe5Imagens_DeveLancarBusinessException()
        {
            var imagens = Enumerable.Range(0, 6).Select(i => CriarImagemFake($"img{i}.jpg")).ToList();
            var dto = CriarDtoValido(imagens);

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Equal("O anúncio não pode ter mais de 5 imagens.", ex.Message);
        }

        [Fact]
        public async Task CriarAnuncioAsync_DadosValidos_DeveCriarComSucesso()
        {
            var dto = CriarDtoValido();

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);
            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>())).ReturnsAsync(true);
            _uploadService.Setup(x => x.UploadAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("foto.jpg");

            await _sut.CriarAnuncioAsync(dto, VENDEDOR_ID);

            _anuncioRepo.Verify(x => x.AdicionarAsync(It.IsAny<Anuncio>()), Times.Once);
        }

        [Fact]
        public async Task CriarAnuncioAsync_ComTipoDeImagemInvalida_DeveLancarBusinessException()
        {
            var dto = CriarDtoValido();

            _clienteRepo.Setup(x => x.ExisteAsync(VENDEDOR_ID)).ReturnsAsync(true);
            _livroRepo.Setup(x => x.ExisteAsync(dto.LivroIsbn)).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(dto.CategoriaId)).ReturnsAsync(true);
            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>())).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.CriarAnuncioAsync(dto, VENDEDOR_ID));

            Assert.Contains("não parece conter um livro", ex.Message, StringComparison.OrdinalIgnoreCase);
            _anuncioRepo.Verify(x => x.AdicionarAsync(It.IsAny<Anuncio>()), Times.Never);
        }

        [Fact]
        public async Task RemoverAnuncioAsync_QuandoAnuncioNaoExiste_DeveLancarNotFoundException()
        {
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(999L)).ReturnsAsync((Anuncio?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.RemoverAnuncioAsync(999, VENDEDOR_ID, null));

            Assert.Equal("O anúncio com ID 999 não foi encontrado.", ex.Message);
        }

        [Fact]
        public async Task RemoverAnuncioAsync_ComTransacaoAtiva_DeveLancarBusinessException()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            var pedidos = new List<PedidoTransacao>
            {
                new PedidoTransacao(anuncio.Id, 20, TipoAnuncio.VENDA, anuncio.Id, VENDEDOR_ID, VENDEDOR_ID)
            };

            var cliente = CriarClienteAdmin(VENDEDOR_ID, false);

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(x => x.ObterPorIdAsync(VENDEDOR_ID)).ReturnsAsync(cliente);
            _pedidoRepo.Setup(x => x.ObterPorAnuncioIdAsync(anuncio.Id)).ReturnsAsync(pedidos);
            _transacaoRepo.Setup(x => x.ExisteTransacaoAtivaAsync(It.IsAny<List<long>>()))
                          .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.RemoverAnuncioAsync(anuncio.Id, VENDEDOR_ID, null));

            Assert.Equal("Não podes apagar um anúncio com transações ativas", ex.Message);
        }

        [Fact]
        public async Task RemoverAnuncioAsync_ComPedidosSemTransacoes_DeveRemoverTudo()
        {
            var anuncio = Anuncio.CriarAnuncio(25, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            var pedidos = new List<PedidoTransacao>
            {
                new PedidoTransacao(1, 25, TipoAnuncio.VENDA, anuncio.Id, VENDEDOR_ID, VENDEDOR_ID)
            };

            var cliente = CriarClienteAdmin(VENDEDOR_ID, false);

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(x => x.ObterPorIdAsync(VENDEDOR_ID)).ReturnsAsync(cliente);
            _pedidoRepo.Setup(x => x.ObterPorAnuncioIdAsync(anuncio.Id)).ReturnsAsync(pedidos);
            _transacaoRepo.Setup(x => x.ExisteTransacaoAtivaAsync(It.IsAny<List<long>>()))
                          .ReturnsAsync(false);

            await _sut.RemoverAnuncioAsync(anuncio.Id, VENDEDOR_ID, null);

            _pedidoRepo.Verify(x => x.RemoverRangeAsync(pedidos), Times.Once);
            _anuncioRepo.Verify(x => x.RemoverAsync(anuncio), Times.Once);
        }

        [Fact]
        public async Task RemoverAnuncioAsync_SemPedidosOuTransacoes_DeveRemoverComSucesso()
        {
            var anuncio = Anuncio.CriarAnuncio(40, 123, 1, VENDEDOR_ID, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img1.jpg;img2.jpg");

            var cliente = CriarClienteAdmin(VENDEDOR_ID, false);

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(x => x.ObterPorIdAsync(VENDEDOR_ID)).ReturnsAsync(cliente);
            _pedidoRepo.Setup(x => x.ObterPorAnuncioIdAsync(anuncio.Id)).ReturnsAsync(new List<PedidoTransacao>());
            _transacaoRepo.Setup(x => x.ExisteTransacaoAtivaAsync(It.IsAny<List<long>>()))
                          .ReturnsAsync(false);

            await _sut.RemoverAnuncioAsync(anuncio.Id, VENDEDOR_ID, null);

            _anuncioRepo.Verify(x => x.RemoverAsync(anuncio), Times.Once);
        }


        [Fact]
        public async Task RemoverAnuncioAsync_AtravesDoAdmin_DeveRemoverComSucessoECriarNotificacaoParaOClientQueAnunciou()
        {
            var anuncio = Anuncio.CriarAnuncio(40, 123, 1, 1, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img1.jpg;img2.jpg");

            var admin = CriarClienteAdmin(ADMIN_ID, true);

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(x => x.ObterPorIdAsync(ADMIN_ID)).ReturnsAsync(admin);
            _pedidoRepo.Setup(x => x.ObterPorAnuncioIdAsync(anuncio.Id)).ReturnsAsync(new List<PedidoTransacao>());
            _transacaoRepo.Setup(x => x.ExisteTransacaoAtivaAsync(It.IsAny<List<long>>())).ReturnsAsync(false);

            _notificacaoService
                .Setup(n => n.CriarNotificacaoAsync("Não Gostei!", TipoNotificacao.SISTEMA, anuncio.VendedorId))
                .ReturnsAsync(new Notificacao());

            await _sut.RemoverAnuncioAsync(anuncio.Id, ADMIN_ID, "Não Gostei!");

            _notificacaoService.Verify(n =>
                n.CriarNotificacaoAsync("Não Gostei!", TipoNotificacao.SISTEMA, anuncio.VendedorId),
                Times.Once);

            _uploadService.Verify(u => u.DeleteImgAsync("img1.jpg"), Times.Once);
            _uploadService.Verify(u => u.DeleteImgAsync("img2.jpg"), Times.Once);

            _anuncioRepo.Verify(x => x.RemoverAsync(anuncio), Times.Once);
        }

        [Fact]
        public async Task RemoverAnuncioAsync_UtilizadorNaoDonoENaoAdmin_DeveLancarBusinessException()
        {
            var anuncio = Anuncio.CriarAnuncio(30, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            var outroUser = CriarClienteAdmin(OUTRO_USER, false); 
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _clienteRepo.Setup(x => x.ObterPorIdAsync(OUTRO_USER)).ReturnsAsync(outroUser);

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.RemoverAnuncioAsync(anuncio.Id, OUTRO_USER, null));

            Assert.Equal("Não tens permissões para apagar este anúncio!", ex.Message);
        }

        [Fact]
        public async Task EditarAnuncioAsync_QuandoAnuncioNaoExiste_DeveLancarNotFoundException()
        {
            _anuncioRepo.Setup(x => x.ObterPorIdAsync(It.IsAny<long>()))
                .ReturnsAsync((Anuncio?)null);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 50,
                LivroIsbn = 123,
                CategoriaId = 1
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.EditarAnuncioAsync(99, dto, VENDEDOR_ID));

            Assert.Equal("O anúncio com ID 99 não existe.", ex.Message);
        }

        [Fact]
        public async Task EditarAnuncioAsync_ComDonoIncorreto_DeveLancarBusinessException()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 30,
                LivroIsbn = 123,
                CategoriaId = 1
            };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.EditarAnuncioAsync(anuncio.Id, dto, OUTRO_USER));

            Assert.Equal("Apenas o vendedor pode modificar o seu anúncio", ex.Message);
        }

        [Fact]
        public async Task EditarAnuncioAsync_ComLivroInexistente_DeveLancarNotFoundException()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(false);
            _bookInfoService.Setup(x => x.ObterLivroPorISBNAsync(It.IsAny<string>()))
                            .ReturnsAsync((LivroDTO?)null);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 30,
                LivroIsbn = 999,
                CategoriaId = 1
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.EditarAnuncioAsync(anuncio.Id, dto, VENDEDOR_ID));

            Assert.Equal("O livro especificado não existe.", ex.Message);
        }

        [Fact]
        public async Task EditarAnuncioAsync_ComCategoriaInexistente_DeveLancarNotFoundException()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(false);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 25,
                LivroIsbn = 123,
                CategoriaId = 999
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.EditarAnuncioAsync(anuncio.Id, dto, VENDEDOR_ID));

            Assert.Equal("A categoria especificada não existe.", ex.Message);
        }

        [Fact]
        public async Task EditarAnuncioAsync_PrecoNegativo_DeveLancarBusinessException()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = -5,
                LivroIsbn = 123,
                CategoriaId = 1
            };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.EditarAnuncioAsync(anuncio.Id, dto, VENDEDOR_ID));

            Assert.Equal("Não pode ter preços negativos.", ex.Message);
        }

        [Fact]
        public async Task EditarAnuncioAsync_EstadoLivroInvalido_DeveLancarDomainException()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 30,
                LivroIsbn = 123,
                CategoriaId = 1,
                EstadoLivro = (EstadoLivro)999 
            };

            await Assert.ThrowsAsync<DomainException>(() =>
                Task.Run(() => anuncio.AtualizarAnuncio(dto.Preco, dto.LivroIsbn, dto.CategoriaId, dto.EstadoLivro, TipoAnuncio.VENDA, anuncio.Imagens)));
        }

        [Fact]
        public async Task EditarAnuncioAsync_TipoAnuncioInvalido_DeveLancarDomainException()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 30,
                LivroIsbn = 123,
                CategoriaId = 1,
                TipoAnuncio = (TipoAnuncio)999 
            };

            await Assert.ThrowsAsync<DomainException>(() =>
                Task.Run(() => anuncio.AtualizarAnuncio(dto.Preco, dto.LivroIsbn, dto.CategoriaId, EstadoLivro.USADO, dto.TipoAnuncio, anuncio.Imagens)));
        }

        [Fact]
        public async Task EditarAnuncioAsync_RemovendoTodasAsImagens_DeveLancarBusinessException()
        {
            var anuncio = Anuncio.CriarAnuncio(30, 123, 1, VENDEDOR_ID, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img1.jpg;img2.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 35,
                LivroIsbn = 123,
                CategoriaId = 1,
                ImagensRemover = new List<string> { "img1.jpg", "img2.jpg" },
                NovasImagens = new List<IFormFile>()
            };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.EditarAnuncioAsync(anuncio.Id, dto, VENDEDOR_ID));

            Assert.Equal("O anúncio deve conter pelo menos uma imagem.", ex.Message);
        }

        [Fact]
        public async Task EditarAnuncioAsync_ComMaisDe5Imagens_DeveLancarBusinessException()
        {
            var imagensExistentes = string.Join(";", Enumerable.Range(1, 5).Select(i => $"img{i}.jpg"));
            var anuncio = Anuncio.CriarAnuncio(30, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, imagensExistentes);

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>())).ReturnsAsync(true);
            _uploadService.Setup(x => x.UploadAsync(It.IsAny<IFormFile>())).ReturnsAsync("nova.jpg");

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 35,
                LivroIsbn = 123,
                CategoriaId = 1,
                NovasImagens = new List<IFormFile> { CriarImagemFake("nova.jpg") }
            };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.EditarAnuncioAsync(anuncio.Id, dto, VENDEDOR_ID));

            Assert.Contains("não pode ter mais de 5 imagens", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task EditarAnuncioAsync_ImagemSemLivro_DeveLancarBusinessException()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img1.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);

            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>())).ReturnsAsync(false);

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 30,
                LivroIsbn = 123,
                CategoriaId = 1,
                NovasImagens = new List<IFormFile> { CriarImagemFake("foto_errada.jpg") }
            };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.EditarAnuncioAsync(anuncio.Id, dto, VENDEDOR_ID));

            Assert.Contains("não parece conter um livro", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task EditarAnuncioAsync_DadosValidos_DeveAtualizar()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, VENDEDOR_ID, EstadoLivro.USADO, TipoAnuncio.VENDA, "img1.jpg;img2.jpg");

            _anuncioRepo.Setup(x => x.ObterPorIdAsync(anuncio.Id)).ReturnsAsync(anuncio);
            _livroRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _categoriaRepo.Setup(x => x.ExisteAsync(It.IsAny<long>())).ReturnsAsync(true);
            _imagemService.Setup(x => x.VerificarSeELivroAsync(It.IsAny<IFormFile>())).ReturnsAsync(true);
            _uploadService.Setup(x => x.UploadAsync(It.IsAny<IFormFile>())).ReturnsAsync("nova.jpg");

            var dto = new AtualizarAnuncioDTO
            {
                Preco = 50,
                LivroIsbn = 123,
                CategoriaId = 1,
                EstadoLivro = EstadoLivro.NOVO,
                TipoAnuncio = TipoAnuncio.VENDA,
                ImagensRemover = new List<string> { "img2.jpg" },
                NovasImagens = new List<IFormFile> { CriarImagemFake("nova.jpg") }
            };

            await _sut.EditarAnuncioAsync(anuncio.Id, dto, VENDEDOR_ID);

            _anuncioRepo.Verify(x => x.Atualizar(It.IsAny<Anuncio>()), Times.Once);
        }

        [Fact]
        public async Task PesquisarAnunciosAsync_QuandoSemResultadosComFiltros_DeveLancarNotFoundException()
        {
            var livro = new Livro(123, "Livro Teste", "Autor Teste");
            var categoria = CriarCategoria(1, "Ficção");
            
            var anuncios = new List<Anuncio>
            {
                Anuncio.CriarAnuncio(50, 123, 1, 1, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img.jpg")
            };

            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[0], livro);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[0], categoria);

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync()).ReturnsAsync(anuncios);

            var filtro = new FiltroAnuncioDTO
            {
                PrecoMinimo = 200, 
                PrecoMaximo = 300
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.PesquisarAnunciosAsync(filtro));
            Assert.Equal("Nenhum anúncio encontrado para este filtro", ex.Message);
        }

        [Fact]
        public async Task PesquisarAnunciosAsync_QuandoPrecoMinimoMaiorQueMaximo_DeveLancarBusinessException()
        {
            var livro = new Livro(123, "Livro Teste", "Autor Teste");
            var categoria = CriarCategoria(1, "Ficção");
            
            var anuncios = new List<Anuncio>
            {
                Anuncio.CriarAnuncio(40, 123, 1, 1, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg")
            };

            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[0], livro);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[0], categoria);

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync()).ReturnsAsync(anuncios);

            var filtro = new FiltroAnuncioDTO
            {
                PrecoMinimo = 100,
                PrecoMaximo = 50 
            };

            var ex = await Assert.ThrowsAsync<BusinessException>(() => _sut.PesquisarAnunciosAsync(filtro));
            Assert.Equal("O valor mínimo não pode ser maior que o valor máximo!", ex.Message);
        }

        [Fact]
        public async Task PesquisarAnunciosAsync_QuandoFiltraPorTipoAnuncio_DeveRetornarSomenteDoTipo()
        {
            var livro1 = new Livro(111, "História de Portugal", "A. Silva");
            var livro2 = new Livro(222, "Romance do Porto", "B. Sousa");
            var categoria1 = CriarCategoria(1, "História");
            var categoria2 = CriarCategoria(2, "Romance");

            var anuncios = new List<Anuncio>
            {
                Anuncio.CriarAnuncio(45, 111, 1, 1, EstadoLivro.USADO, TipoAnuncio.VENDA, "img1.jpg"),
                Anuncio.CriarAnuncio(0, 222, 2, 2, EstadoLivro.NOVO, TipoAnuncio.DOACAO, "img2.jpg")
            };

            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[0], livro1);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[0], categoria1);
            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[1], livro2);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[1], categoria2);

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync()).ReturnsAsync(anuncios);
            _anuncioRepo.Setup(x => x.ContarFavoritosAsync(It.IsAny<long>())).ReturnsAsync(3);

            var filtro = new FiltroAnuncioDTO { TipoAnuncio = TipoAnuncio.DOACAO };

            var resultado = await _sut.PesquisarAnunciosAsync(filtro);

            Assert.Single(resultado);
            Assert.Equal(TipoAnuncio.DOACAO, resultado.First().TipoAnuncio);
            Assert.Equal("Romance do Porto", resultado.First().Titulo);
        }

        [Fact]
        public async Task PesquisarAnunciosAsync_QuandoAplicaTermoPesquisa_DeveRetornarApenasRelevantes()
        {
            var livro1 = new Livro(111, "Harry Potter e a Pedra Filosofal", "J.K. Rowling");
            var livro2 = new Livro(222, "O Senhor dos Anéis", "Tolkien");
            var categoria1 = CriarCategoria(1, "Fantasia");
            var categoria2 = CriarCategoria(2, "Ficção");

            var anuncios = new List<Anuncio>
            {
                Anuncio.CriarAnuncio(40, 111, 1, 1, EstadoLivro.USADO, TipoAnuncio.VENDA, "img1.jpg"),
                Anuncio.CriarAnuncio(50, 222, 2, 2, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img2.jpg")
            };

            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[0], livro1);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[0], categoria1);
            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[1], livro2);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[1], categoria2);

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync()).ReturnsAsync(anuncios);
            _anuncioRepo.Setup(x => x.ContarFavoritosAsync(It.IsAny<long>())).ReturnsAsync(1);

            var filtro = new FiltroAnuncioDTO { TermoPesquisa = "Harry" };

            var resultado = await _sut.PesquisarAnunciosAsync(filtro);

            Assert.Single(resultado);
            Assert.Equal("Harry Potter e a Pedra Filosofal", resultado.First().Titulo);
        }

        [Fact]
        public async Task PesquisarAnunciosAsync_QuandoTodosFiltrosValidos_DeveRetornarListaFiltrada()
        {
            var livro = new Livro(999, "História de Portugal", "José Rocha");
            var categoria = CriarCategoria(1, "História");
            var anuncio = Anuncio.CriarAnuncio(45, 999, 1, 1, EstadoLivro.USADO, TipoAnuncio.VENDA, "img1.jpg");
            
            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncio, livro);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncio, categoria);

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync())
                .ReturnsAsync(new List<Anuncio> { anuncio });

            _anuncioRepo.Setup(x => x.ContarFavoritosAsync(anuncio.Id))
                .ReturnsAsync(10);

            var filtro = new FiltroAnuncioDTO
            {
                CategoriaId = 1,
                TipoAnuncio = TipoAnuncio.VENDA,
                EstadoLivro = EstadoLivro.USADO,
                PrecoMinimo = 20,
                PrecoMaximo = 50,
                TermoPesquisa = "Portugal"
            };

            var resultado = await _sut.PesquisarAnunciosAsync(filtro);

            Assert.Single(resultado);
            var dto = resultado.First();
            Assert.Equal("História de Portugal", dto.Titulo);
            Assert.Equal(45, dto.Preco);
            Assert.Equal(10, dto.TotalFavoritos);
        }

        [Fact]
        public async Task PesquisarAnunciosAsync_SemFiltros_ComAnuncios_DeveRetornarTodos()
        {
            var livro1 = new Livro(111, "Livro A", "Autor A");
            var livro2 = new Livro(222, "Livro B", "Autor B");
            var categoria1 = CriarCategoria(1, "Categoria A");
            var categoria2 = CriarCategoria(2, "Categoria B");

            var anuncios = new List<Anuncio>
            {
                Anuncio.CriarAnuncio(20, 111, 1, 1, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img1.jpg"),
                Anuncio.CriarAnuncio(30, 222, 2, 2, EstadoLivro.USADO, TipoAnuncio.VENDA, "img2.jpg")
            };

            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[0], livro1);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[0], categoria1);
            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[1], livro2);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[1], categoria2);

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync()).ReturnsAsync(anuncios);
            _anuncioRepo.Setup(x => x.ContarFavoritosAsync(It.IsAny<long>())).ReturnsAsync(0);

            var filtro = new FiltroAnuncioDTO(); 

            var resultado = await _sut.PesquisarAnunciosAsync(filtro);

            Assert.Equal(2, resultado.Count);
        }

        [Fact]
        public async Task PesquisarAnunciosAsync_SemFiltros_SemAnuncios_DeveLancarNotFoundException()
        {
            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync())
                .ReturnsAsync(new List<Anuncio>()); 

            var filtro = new FiltroAnuncioDTO();

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.PesquisarAnunciosAsync(filtro));

            Assert.Equal("Ainda não existem anúncios", ex.Message);
        }

        [Fact]
        public async Task PesquisarAnunciosAsync_CategoriaSemResultados_DeveLancarNotFoundException()
        {
            var livro = new Livro(111, "Livro A", "Autor A");
            var categoria = CriarCategoria(1, "Categoria A");

            var anuncios = new List<Anuncio>
            {
                Anuncio.CriarAnuncio(20, 111, 1, 1, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img1.jpg")
            };

            typeof(Anuncio).GetProperty("Livro")!.SetValue(anuncios[0], livro);
            typeof(Anuncio).GetProperty("Categoria")!.SetValue(anuncios[0], categoria);

            _anuncioRepo.Setup(x => x.ObterAtivosComLivroEVendedorAsync())
                .ReturnsAsync(anuncios);

            _anuncioRepo.Setup(x => x.ContarFavoritosAsync(It.IsAny<long>())).ReturnsAsync(0);

            var filtro = new FiltroAnuncioDTO { CategoriaId = 999 }; 

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.PesquisarAnunciosAsync(filtro));

            Assert.Equal("Nenhum anúncio encontrado para este filtro", ex.Message);
        }

        private IFormFile CriarImagemFake(string nome)
        {
            var bytes = new byte[100];
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, "file", nome)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }

        private CriarAnuncioDTO CriarDtoValido(List<IFormFile>? imagens = null)
        {
            return new CriarAnuncioDTO
            {
                Preco = 20,
                LivroIsbn = 123,
                CategoriaId = 1,
                EstadoLivro = EstadoLivro.NOVO,
                TipoAnuncio = TipoAnuncio.VENDA,
                Imagens = imagens ?? new List<IFormFile> { CriarImagemFake("foto.jpg") }
            };
        }

        private Cliente CriarClienteAdmin(long id, bool admin)
        {
            var cliente = (Cliente)Activator.CreateInstance(typeof(Cliente), nonPublic: true)!;

            typeof(Cliente).GetProperty("Id")!.SetValue(cliente, id);
            typeof(Cliente).GetProperty("Nome")!.SetValue(cliente, "Admin User");
            typeof(Cliente).GetProperty("Email")!.SetValue(cliente, "admin@test.com");
            typeof(Cliente).GetProperty("PasswordHash")!.SetValue(cliente, "hash");
            typeof(Cliente).GetProperty("Telefone")!.SetValue(cliente, "912345678");
            typeof(Cliente).GetProperty("Dob")!.SetValue(cliente, DateTime.Now.AddYears(-20));
            typeof(Cliente).GetProperty("Pontos")!.SetValue(cliente, 0);
            typeof(Cliente).GetProperty("IsAdmin")!.SetValue(cliente, admin);

            return cliente;
        }

        private Categoria CriarCategoria(long id, string nome)
        {
            var categoria = (Categoria)Activator.CreateInstance(typeof(Categoria), nonPublic: true)!;

            typeof(Categoria).GetProperty("Id")!.SetValue(categoria, id);
            typeof(Categoria).GetProperty("Nome")!.SetValue(categoria, nome);
            typeof(Categoria).GetProperty("Ativo")!.SetValue(categoria, true);

            return categoria;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using Moq;

namespace BookFlaz.Tests.Application.Services.Unitarios
{
    public class CategoriaServiceTestes
    {
        private readonly Mock<ICategoriaRepository> _repo = new();
        private readonly CategoriaService _sut;

        public CategoriaServiceTestes()
        {
            _sut = new CategoriaService(_repo.Object);
        }
        [Fact]
        public async Task CriarCategoriaAsync_ComDadosValidos_DeveCriar()
        {
            var dto = new CriarCategoriaDTO { Nome = "Ficção", Ativo = true };

            _repo.Setup(r => r.ObterPorNomeAsync("Ficção"))
                 .ReturnsAsync((Categoria?)null);
            _repo.Setup(r => r.AdicionarAsync(It.IsAny<Categoria>()))
                 .Returns(Task.CompletedTask);

            var cat = await _sut.CriarCategoriaAsync(dto);

            Assert.Equal("Ficção", cat.Nome);
            Assert.True(cat.Ativo);
            _repo.Verify(r => r.AdicionarAsync(It.IsAny<Categoria>()), Times.Once);
        }

        [Fact]
        public async Task CriarCategoriaAsync_ComNomeDuplicado_DeveLancarExcecao()
        {
            var dto = new CriarCategoriaDTO { Nome = "Ficção", Ativo = true };

            _repo.Setup(r => r.ExisteComMesmoNomeAsync("Ficção", null))
                .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.CriarCategoriaAsync(dto));
            Assert.Equal("Já existe uma categoria com esse nome.", ex.Message);
            _repo.Verify(r => r.AdicionarAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task CriarCategoriaAsync_ComNomeVazio_DeveLancarExcecao()
        {
            var dto = new CriarCategoriaDTO { Nome = "", Ativo = true };

            _repo.Setup(r => r.ObterPorNomeAsync(""))
                 .ReturnsAsync((Categoria?)null);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CriarCategoriaAsync(dto));
            Assert.Equal("O nome da categoria não pode ser vazio ou nulo.", ex.Message);
            _repo.Verify(r => r.AdicionarAsync(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task CriarCategoriaAsync_SemAtivo_DeveDefinirTruePorPadrao()
        {
            var dto = new CriarCategoriaDTO { Nome = "Romance", Ativo = null };

            _repo.Setup(r => r.ObterPorNomeAsync("Romance"))
                 .ReturnsAsync((Categoria?)null);

            Categoria? capturada = null;
            _repo.Setup(r => r.AdicionarAsync(It.IsAny<Categoria>()))
                 .Callback<Categoria>(c => capturada = c)
                 .Returns(Task.CompletedTask);

            var cat = await _sut.CriarCategoriaAsync(dto);

            Assert.NotNull(capturada);
            Assert.Equal("Romance", capturada!.Nome);
            Assert.True(capturada.Ativo);
            Assert.True(cat.Ativo);
        }

        [Fact]
        public async Task EditarCategoriaAsync_ComDadosValidos_DeveEditar()
        {
            var existente = Categoria.CriarCategoria("Ficção", true);
            typeof(Categoria).GetProperty("Id")!.SetValue(existente, 10L);

            var dto = new EditarCategoriaDTO { Nome = "História", Ativo = false };

            _repo.Setup(r => r.ExisteComMesmoNomeAsync("História", 10))
                 .ReturnsAsync(false);
            _repo.Setup(r => r.ObterPorIdAsync(10))
                 .ReturnsAsync(existente);
            _repo.Setup(r => r.Atualizar(It.IsAny<Categoria>()));

            var editada = await _sut.EditarCategoriaAsync(dto, 10);

            Assert.Equal("História", editada.Nome);
            Assert.False(editada.Ativo);
            _repo.Verify(r => r.Atualizar(It.IsAny<Categoria>()), Times.Once);
        }

        [Fact]
        public async Task EditarCategoriaAsync_ComNomeDuplicado_DeveLancarExcecao()
        {
            var dto = new EditarCategoriaDTO { Nome = "Ficção", Ativo = true };

            _repo.Setup(r => r.ExisteComMesmoNomeAsync("Ficção", 20))
                 .ReturnsAsync(true);
            _repo.Setup(r => r.ObterPorIdAsync(20))
                 .ReturnsAsync(new Categoria());

            var ex = await Assert.ThrowsAsync<BookFlaz.Application.Exceptions.ValidationException>(() => _sut.EditarCategoriaAsync(dto, 20));
            Assert.Equal("Já existe uma categoria com esse nome.", ex.Message);
            _repo.Verify(r => r.Atualizar(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task EditarCategoriaAsync_CategoriaInexistente_DeveLancarExcecao()
        {
            var dto = new EditarCategoriaDTO { Nome = "Qualquer", Ativo = true };

            _repo.Setup(r => r.ExisteComMesmoNomeAsync("Qualquer", 999))
                 .ReturnsAsync(false);
            _repo.Setup(r => r.ObterPorIdAsync(999))
                 .ReturnsAsync((Categoria?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.EditarCategoriaAsync(dto, 999));
            Assert.Equal("Categoria não encontrada.", ex.Message);
        }

        [Fact]
        public async Task EditarCategoriaAsync_ComNomeInvalido_DeveLancarExcecao()
        {
            var existente = Categoria.CriarCategoria("Ficção", true);
            typeof(Categoria).GetProperty("Id")!.SetValue(existente, 30L);

            var dto = new EditarCategoriaDTO { Nome = "Ficção123!", Ativo = false };

            _repo.Setup(r => r.ExisteComMesmoNomeAsync("Ficção123!", 30))
                 .ReturnsAsync(false);
            _repo.Setup(r => r.ObterPorIdAsync(30))
                 .ReturnsAsync(existente);

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _sut.EditarCategoriaAsync(dto, 30));
            Assert.Equal("Erro inesperado ao editar a categoria.", ex.Message);
            _repo.Verify(r => r.Atualizar(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task RemoverCategoriaAsync_IdValido_DeveRemover()
        {
            _repo.Setup(r => r.ObterPorIdAsync(5))
                 .ReturnsAsync(new Categoria());
            _repo.Setup(r => r.TemAnunciosVinculadosAsync(5))
                 .ReturnsAsync(false);
            _repo.Setup(r => r.Remover(It.IsAny<Categoria>()));

            var ok = await _sut.RemoverCategoriaAsync(5);

            Assert.True(ok);
            _repo.Verify(r => r.Remover(It.IsAny<Categoria>()), Times.Once);
        }

        [Fact]
        public async Task RemoverCategoriaAsync_IdInvalido_DeveRetornarFalse()
        {
            _repo.Setup(r => r.ObterPorIdAsync(999))
                 .ReturnsAsync((Categoria?)null);

            var ok = await _sut.RemoverCategoriaAsync(999);

            Assert.False(ok);
            _repo.Verify(r => r.Remover(It.IsAny<Categoria>()), Times.Never);
        }

        [Fact]
        public async Task RemoverCategoriaAsync_ComAnunciosVinculados_DeveLancarExcecao()
        {
            _repo.Setup(r => r.ObterPorIdAsync(1))
                 .ReturnsAsync(new Categoria());
            _repo.Setup(r => r.TemAnunciosVinculadosAsync(1))
                 .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _sut.RemoverCategoriaAsync(1));
            Assert.Equal("Não é possível remover: existem anúncios vinculados à categoria.", ex.Message);
        }

        [Fact]
        public async Task VisualizarCategorias_SemFiltros_DeveRetornarTodas()
        {
            var lista = new List<Categoria>
            {
                Categoria.CriarCategoria("Ficção", true),
                Categoria.CriarCategoria("História", false)
            };

            _repo.Setup(r => r.ListarAsync(null, null)).ReturnsAsync(lista);

            var result = await _sut.VisualizarCategoriasAsync(null, null);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task VisualizarCategorias_ComFiltros_DeveRetornarFiltradas()
        {
            var filtrada = new List<Categoria>
            {
                Categoria.CriarCategoria("Ficção", true)
            };

            _repo.Setup(r => r.ListarAsync("Fic", true)).ReturnsAsync(filtrada);

            var result = await _sut.VisualizarCategoriasAsync("Fic", true);

            Assert.Single(result);
            Assert.Equal("Ficção", result[0].Nome);
            Assert.True(result[0].Ativo);
        }
    }
}

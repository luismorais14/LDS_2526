using BookFlaz.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace BookFlaz.Tests.Domain.Entities
{
    public class CategoriaTestes
    {

        [Fact]
        public void CriarCategoria_ComDadosValidos_DeveRetornarCategoria()
        {
            string nome = "Ficção";
            bool ativo = true;

            var categoria = Categoria.CriarCategoria(nome, ativo);

            Assert.Equal(nome, categoria.Nome);
            Assert.Equal(ativo, categoria.Ativo);
        }

        [Fact]
        public void CriarCategoria_SemValorAtivo_DeveDefinirAtivoComoTrue()
        {
            string nome = "Romance";
            bool? ativo = null;

            var categoria = Categoria.CriarCategoria(nome, ativo);

            Assert.Equal(nome, categoria.Nome);
            Assert.True(categoria.Ativo);
        }

        [Fact]
        public void CriarCategoria_ComAtivoFalso_DeveManterAtivoFalso()
        {
            string nome = "Terror";
            bool ativo = false;

            var categoria = Categoria.CriarCategoria(nome, ativo);

            Assert.Equal(nome, categoria.Nome);
            Assert.False(categoria.Ativo);
        }

        [Fact]
        public void CriarCategoria_ComNomeVazio_DeveLancarExcecao()
        {
            string nome = "";
            bool ativo = true;

            var ex = Assert.Throws<ArgumentException>(() => Categoria.CriarCategoria(nome, ativo));
            Assert.Equal("O nome da categoria não pode ser vazio ou nulo.", ex.Message);
        }

        [Fact]
        public void CriarCategoria_ComNomeNulo_DeveLancarExcecao()
        {
            string nome = null;
            bool ativo = true;

            var ex = Assert.Throws<ArgumentException>(() => Categoria.CriarCategoria(nome, ativo));
            Assert.Equal("O nome da categoria não pode ser vazio ou nulo.", ex.Message);
        }

        [Fact]
        public void CriarCategoria_ComNomeExcedendo255Caracteres_DeveLancarExcecao()
        {
            string nome = new string('a', 256);
            bool ativo = true;

            var ex = Assert.Throws<ValidationException>(() => Categoria.CriarCategoria(nome, ativo));
            Assert.Equal("O nome da categoria não pode exceder 255 caracteres.", ex.Message);
        }

        [Fact]
        public void CriarCategoria_ComNomeInvalido_DeveLancarExcecao()
        {
            string nome = "Ficção123!";
            bool ativo = true;

            var ex = Assert.Throws<ValidationException>(() => Categoria.CriarCategoria(nome, ativo));
            Assert.Equal("O nome da categoria deve conter apenas letras.", ex.Message);
        }

        [Fact]
        public void CriarCategoria_ComNomeSomenteEspacos_DeveLancarExcecao()
        {
            string nome = "   ";
            bool ativo = true;

            var ex = Assert.Throws<ArgumentException>(() => Categoria.CriarCategoria(nome, ativo));
            Assert.Equal("O nome da categoria não pode ser vazio ou nulo.", ex.Message);
        }


        [Fact]
        public void EditarCategoria_ComDadosValidos_DeveAtualizarNomeEAtivo()
        {
            var categoria = new Categoria();
            var novoNome = "Ficcao";
            var novoAtivo = false;

            var categoriaAtualizada = Categoria.EditarCategoria(novoNome, novoAtivo, categoria);

            Assert.Equal(novoNome, categoriaAtualizada.Nome);
            Assert.Equal(novoAtivo, categoriaAtualizada.Ativo);
        }

        [Fact]
        public void EditarCategoria_ComNomeNulo_DeveLancarExcecao()
        {
            var categoria = new Categoria();
            string? novoNome = null;

            var ex = Assert.Throws<ArgumentException>(() => Categoria.EditarCategoria(novoNome, null, categoria));
            Assert.Contains("O nome da categoria é obrigatório.", ex.Message);
        }

        [Fact]
        public void EditarCategoria_ComNomeInvalido_DeveLancarExcecao()
        {
            var categoria = new Categoria();
            var novoNome = "Ficcao123";

            var ex = Assert.Throws<ValidationException>(() => Categoria.EditarCategoria(novoNome, null, categoria));
            Assert.Equal("O nome da categoria deve conter apenas letras.", ex.Message);
        }

        [Fact]
        public void EditarCategoria_ComNomeVazio_DeveLancarExcecao()
        {
            var categoria = new Categoria();
            var novoNome = "   ";

            var ex = Assert.Throws<ArgumentException>(() => Categoria.EditarCategoria(novoNome, null, categoria));
            Assert.Contains("O nome da categoria é obrigatório.", ex.Message);
        }

        [Fact]
        public void EditarCategoria_ComNomeMuitoLongo_DeveLancarExcecao()
        {
            var categoria = new Categoria();
            var novoNome = new string('a', 256);

            var ex = Assert.Throws<ValidationException>(() => Categoria.EditarCategoria(novoNome, null, categoria));
            Assert.Equal("O nome da categoria não pode exceder 255 caracteres.", ex.Message);
        }

        [Fact]
        public void EditarCategoria_ComNomeMuitoCurto_DeveLancarExcecao()
        {
            var categoria = new Categoria();
            var novoNome = "";

            var ex = Assert.Throws<ArgumentException>(() => Categoria.EditarCategoria(novoNome, null, categoria));
            Assert.Contains("O nome da categoria é obrigatório.", ex.Message);
        }

        [Fact]
        public void EditarCategoria_ComAtivoNulo_NaoDeveAlterarAtivo()
        {
            var categoria = new Categoria();
            var novoNome = "Romance";
            var estadoAtivoOriginal = categoria.Ativo;

            var categoriaAtualizada = Categoria.EditarCategoria(novoNome, null, categoria);

            Assert.Equal(novoNome, categoriaAtualizada.Nome);
            Assert.Equal(estadoAtivoOriginal, categoriaAtualizada.Ativo);
        }

        [Fact]
        public void EditarCategoria_ComAtivoFalso_DeveAtualizarAtivo()
        {
            var categoria = new Categoria();
            var novoNome = "Aventura";
            var novoAtivo = false;

            var categoriaAtualizada = Categoria.EditarCategoria(novoNome, novoAtivo, categoria);

            Assert.Equal(novoNome, categoriaAtualizada.Nome);
            Assert.Equal(novoAtivo, categoriaAtualizada.Ativo);
        }
    }
}

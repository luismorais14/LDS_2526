using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Tests.Domain.Entities
{
    /// <summary>
    /// Testes unitários para a entidade <see cref="Anuncio"/>, 
    /// </summary>
    public class AnuncioTests
    {
        /// <summary>
        /// Verifica se o método <see cref="Anuncio.criarAnuncio"/> cria um anúncio válido 
        /// com todos os campos corretamente atribuídos.
        /// </summary>
        [Fact]
        public void CriarAnuncio_DeveCriarComValoresValidos()
        {
            decimal preco = 50;
            long livro = 1234567890123;
            long categoria = 1;
            long vendedor = 10;
            string imagens = "img1.jpg;img2.jpg";

            var anuncio = Anuncio.CriarAnuncio(preco, livro, categoria, vendedor, EstadoLivro.NOVO, TipoAnuncio.VENDA, imagens);

            Assert.Equal(preco, anuncio.Preco);
            Assert.Equal(livro, anuncio.LivroIsbn);
            Assert.Equal(categoria, anuncio.CategoriaId);
            Assert.Equal(vendedor, anuncio.VendedorId);
            Assert.Equal(EstadoAnuncio.ATIVO, anuncio.EstadoAnuncio);
            Assert.Equal(imagens, anuncio.Imagens);
            Assert.True(anuncio.DataCriacao <= DateTime.UtcNow);
        }

        /// <summary>
        /// Garante que anúncios do tipo <see cref="TipoAnuncio.DOACAO"/> 
        /// têm sempre o preço igual a zero, independentemente do valor informado.
        /// </summary>
        [Fact]
        public void CriarAnuncio_Doacao_DeveTerPrecoZero()
        {
            var anuncio = Anuncio.CriarAnuncio(100, 123, 1, 1, EstadoLivro.USADO, TipoAnuncio.DOACAO, "img.jpg");

            Assert.Equal(0, anuncio.Preco);
        }

        /// <summary>
        /// Confirma que um novo anúncio é criado com o estado padrão <see cref="EstadoAnuncio.ATIVO"/>.
        /// </summary>
        [Fact]
        public void CriarAnuncio_DeveTerEstadoAtivoPorPadrao()
        {
            var anuncio = Anuncio.CriarAnuncio(10, 123, 1, 1, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img.jpg");

            Assert.Equal(EstadoAnuncio.ATIVO, anuncio.EstadoAnuncio);
        }

        /// <summary>
        /// Verifica se a data de criação é automaticamente definida no momento da criação do anúncio.
        /// </summary>
        [Fact]
        public void CriarAnuncio_DeveDefinirDataCriacao()
        {
            var anuncio = Anuncio.CriarAnuncio(10, 123, 1, 1, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img.jpg");

            Assert.True(anuncio.DataCriacao <= DateTime.UtcNow);
        }

        /// <summary>
        /// Testa se o método <see cref="Anuncio.atualizarAnuncio"/> 
        /// atualiza corretamente todos os campos de um anúncio existente.
        /// </summary>
        [Fact]
        public void AtualizarAnuncio_DeveAtualizarCampos()
        {
            var anuncio = Anuncio.CriarAnuncio(20, 123, 1, 1, EstadoLivro.USADO, TipoAnuncio.VENDA, "img1.jpg");

            anuncio.AtualizarAnuncio(35, 999, 2, EstadoLivro.NOVO, TipoAnuncio.VENDA, "nova1.jpg;nova2.jpg");

            Assert.Equal(35, anuncio.Preco);
            Assert.Equal(999, anuncio.LivroIsbn);
            Assert.Equal(2, anuncio.CategoriaId);
            Assert.Equal(EstadoLivro.NOVO, anuncio.EstadoLivro);
            Assert.Equal("nova1.jpg;nova2.jpg", anuncio.Imagens);
            Assert.NotEqual(default, anuncio.DataAtualizacao);
        }

        /// <summary>
        /// Garante que ao atualizar um anúncio para o tipo <see cref="TipoAnuncio.DOACAO"/>, 
        /// o preço é automaticamente redefinido para zero.
        /// </summary>
        [Fact]
        public void AtualizarAnuncio_Doacao_DeveTerPrecoZero()
        {
            var anuncio = Anuncio.CriarAnuncio(50, 123, 1, 1, EstadoLivro.USADO, TipoAnuncio.VENDA, "img.jpg");

            anuncio.AtualizarAnuncio(99, 123, 1, EstadoLivro.USADO, TipoAnuncio.DOACAO, "img.jpg");

            Assert.Equal(0, anuncio.Preco);
        }

        /// <summary>
        /// Confirma que a data de atualização (<see cref="Anuncio.dataAtualizacao"/>) 
        /// é corretamente atualizada sempre que o anúncio é modificado.
        /// </summary>
        [Fact]
        public void AtualizarAnuncio_DeveAtualizarDataAtualizacao()
        {
            var anuncio = Anuncio.CriarAnuncio(10, 123, 1, 1, EstadoLivro.NOVO, TipoAnuncio.VENDA, "img.jpg");

            anuncio.AtualizarAnuncio(15, 123, 1, EstadoLivro.USADO, TipoAnuncio.VENDA, "nova.jpg");

            Assert.NotEqual(default, anuncio.DataAtualizacao);
            Assert.True(anuncio.DataAtualizacao <= DateTime.UtcNow);
        }


        /// <summary>
        /// Garante que o método <see cref="Anuncio.AtualizarEstadoAnuncio(EstadoAnuncio)"/>
        /// altera corretamente o estado de um anúncio de <see cref="EstadoAnuncio.ATIVO"/> 
        /// para outro estado fornecido.
        /// </summary>
        [Fact]
        public void AtualizarEstadoAnuncio_DeveAlterarEstadoCorretamente()
        {
            var anuncio = Anuncio.CriarAnuncio(
                preco: 20,
                livroIsbn: 1234567890123,
                categoriaId: 1,
                vendedorId: 1,
                estadoLivro: EstadoLivro.USADO,
                tipoAnuncio: TipoAnuncio.VENDA,
                imagens: "foto.jpg"
            );

            Assert.Equal(EstadoAnuncio.ATIVO, anuncio.EstadoAnuncio); 

            anuncio.AtualizarEstadoAnuncio(EstadoAnuncio.VENDIDO);

            Assert.Equal(EstadoAnuncio.VENDIDO, anuncio.EstadoAnuncio);
        }

        /// <summary>
        /// Verifica que o método <see cref="Anuncio.AtualizarEstadoAnuncio(EstadoAnuncio)"/>
        /// permite múltiplas alterações consecutivas de estado sem restrições nem perda de integridade.
        /// </summary>
        [Fact]
        public void AtualizarEstadoAnuncio_DevePermitirMultiplasMudancas()
        {
            var anuncio = Anuncio.CriarAnuncio(
                preco: 50,
                livroIsbn: 9876543210123,
                categoriaId: 2,
                vendedorId: 10,
                estadoLivro: EstadoLivro.NOVO,
                tipoAnuncio: TipoAnuncio.VENDA,
                imagens: "foto2.jpg"
            );

            anuncio.AtualizarEstadoAnuncio(EstadoAnuncio.INDISPONIVEL);
            anuncio.AtualizarEstadoAnuncio(EstadoAnuncio.ATIVO);
            anuncio.AtualizarEstadoAnuncio(EstadoAnuncio.VENDIDO);

            Assert.Equal(EstadoAnuncio.VENDIDO, anuncio.EstadoAnuncio);
        }
    }
}

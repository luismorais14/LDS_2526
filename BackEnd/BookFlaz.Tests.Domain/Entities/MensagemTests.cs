using BookFlaz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookFlaz.Tests.Domain.Entities
{
    public class MensagemTests
    {
        [Fact]
        public void CriarMensagem_ComDadosValidos_DeveRetornarMensagem()
        {
            long clienteId = 1;
            long conversaId = 10;
            string conteudo = "Olá, tudo bem?";
            
            var mensagem = Mensagem.CriarMensagem(clienteId, conversaId, conteudo);

            Assert.Equal(clienteId, mensagem.ClienteId);
            Assert.Equal(conversaId, mensagem.ConversaId);
            Assert.Equal(conteudo, mensagem.Conteudo);
            Assert.True(mensagem.DataEnvio <= DateTime.Now);
        }

        [Fact]
        public void CriarMensagem_ComConteudoVazio_DeveLancarExcecao()
        {
            long clienteId = 1;
            long conversaId = 10;

            var exception = Assert.Throws<ArgumentException>(() =>
                Mensagem.CriarMensagem(clienteId, conversaId, "")
            );

            Assert.Contains("Conteúdo da mensagem não pode ser vazio.", exception.Message);
        }

        [Fact]
        public void CriarMensagem_ComConteudoNulo_DeveLancarExcecao()
        {
            long clienteId = 1;
            long conversaId = 10;

            var exception = Assert.Throws<ArgumentException>(() =>
                Mensagem.CriarMensagem(clienteId, conversaId, null)
            );

            Assert.Contains("Conteúdo da mensagem não pode ser vazio.", exception.Message);
        }

        [Fact]
        public void CriarMensagem_ComConteudoExcedendo1000Caracteres_DeveLancarExcecao()
        {
            long clienteId = 1;
            long conversaId = 10;
            string conteudo = new string('A', 1001);

            var exception = Assert.Throws<ArgumentException>(() =>
                Mensagem.CriarMensagem(clienteId, conversaId, conteudo)
            );

            Assert.Contains("Conteúdo da mensagem não pode exceder 1000 caracteres.", exception.Message);
        }
    }
}

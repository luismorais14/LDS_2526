using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Enums;
using BookFlaz.Domain.Repositories;
using Moq;

namespace BookFlaz.Tests.Application.Services.Unitarios
{
    public class AvaliacaoServiceTestes
    {
        private readonly Mock<IAvaliacaoRepository> _avaliacaoRepo;
        private readonly Mock<ITransacaoRepository> _transacaoRepo;
        private readonly Mock<IClienteRepository> _clienteRepo;
        private readonly AvaliacaoService _sut;

        private const long COMPRADOR_ID = 1;
        private const long VENDEDOR_ID = 2;
        private const long OUTRO_UTILIZADOR_ID = 3;
        private const long TRANSACAO_ID = 99;

        public AvaliacaoServiceTestes()
        {
            _avaliacaoRepo = new Mock<IAvaliacaoRepository>();
            _transacaoRepo = new Mock<ITransacaoRepository>();
            _clienteRepo = new Mock<IClienteRepository>();
            _sut = new AvaliacaoService(_avaliacaoRepo.Object, _transacaoRepo.Object, _clienteRepo.Object);
        }

        private Transacao CriarTransacaoDeTeste(long compradorId, long vendedorId, EstadoTransacao estado)
        {
            var transacao = Transacao.CriarTransacao(
                valorTotal: 50.0,
                pontosUsados: 0,
                pedidoId: 123,
                compradorId: compradorId,
                vendedorId: vendedorId
            );
            
            if (estado != EstadoTransacao.PENDENTE)
            {
                transacao.AtualizarEstado(estado);
            }
            return transacao;
        }
        
        [Fact]
        public async Task TransacaoNaoEncontrada_LancaNotFoundException()
        {
            _transacaoRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<long>()))
                .ReturnsAsync((Transacao)null);
            
            var dto = new AvaliacaoDTO { Estrelas = 5 };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _sut.AvaliarAsync(COMPRADOR_ID, VENDEDOR_ID, TRANSACAO_ID, dto));
            
            Assert.Equal("Transação não encontrada.", ex.Message);
        }

        [Fact]
        public async Task TransacaoNaoConcluida_LancaBusinessException()
        {
            var transacaoPendente = CriarTransacaoDeTeste(COMPRADOR_ID, VENDEDOR_ID, EstadoTransacao.PENDENTE);
            _transacaoRepo.Setup(r => r.ObterPorIdAsync(TRANSACAO_ID))
                .ReturnsAsync(transacaoPendente);
            
            var dto = new AvaliacaoDTO { Estrelas = 5 };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.AvaliarAsync(COMPRADOR_ID, VENDEDOR_ID, TRANSACAO_ID, dto));
            
            Assert.Equal("Transação não concluida.", ex.Message);
        }

        [Fact]
        public async Task UtilizadoresNaoPertencemATransacao_LancaBusinessException()
        {
            var transacaoValida = CriarTransacaoDeTeste(COMPRADOR_ID, VENDEDOR_ID, EstadoTransacao.CONCLUIDA);
            _transacaoRepo.Setup(r => r.ObterPorIdAsync(TRANSACAO_ID))
                .ReturnsAsync(transacaoValida);
            
            var dto = new AvaliacaoDTO { Estrelas = 5 };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.AvaliarAsync(OUTRO_UTILIZADOR_ID, VENDEDOR_ID, TRANSACAO_ID, dto));
            
            Assert.Equal("Os utilizadores não pertencem a esta transação.", ex.Message);
        }

        [Fact]
        public async Task AvaliadorIgualAvaliado_LancaBusinessException()
        {
            var transacaoValida = CriarTransacaoDeTeste(COMPRADOR_ID, VENDEDOR_ID, EstadoTransacao.CONCLUIDA);
            _transacaoRepo.Setup(r => r.ObterPorIdAsync(TRANSACAO_ID))
                .ReturnsAsync(transacaoValida);
            
            var dto = new AvaliacaoDTO { Estrelas = 5 };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.AvaliarAsync(COMPRADOR_ID, COMPRADOR_ID, TRANSACAO_ID, dto));
            
            Assert.Equal("Os utilizadores não pertencem a esta transação.", ex.Message);
        }

        [Fact]
        public async Task AvaliacaoDuplicada_LancaBusinessException()
        {
            var transacaoValida = CriarTransacaoDeTeste(COMPRADOR_ID, VENDEDOR_ID, EstadoTransacao.CONCLUIDA);
            _transacaoRepo.Setup(r => r.ObterPorIdAsync(TRANSACAO_ID))
                .ReturnsAsync(transacaoValida);
            
            _avaliacaoRepo.Setup(r => r.ExisteAsync(TRANSACAO_ID, COMPRADOR_ID))
                .ReturnsAsync(true);
            
            var dto = new AvaliacaoDTO { Estrelas = 5 };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.AvaliarAsync(COMPRADOR_ID, VENDEDOR_ID, TRANSACAO_ID, dto));
            
            Assert.Equal("Já existe avaliação deste utilizador para esta transação.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async Task EstrelasInvalidas_LancaBusinessException(int estrelasInvalidas)
        {
            var transacaoValida = CriarTransacaoDeTeste(COMPRADOR_ID, VENDEDOR_ID, EstadoTransacao.CONCLUIDA);
            _transacaoRepo.Setup(r => r.ObterPorIdAsync(TRANSACAO_ID))
                .ReturnsAsync(transacaoValida);
            
            _avaliacaoRepo.Setup(r => r.ExisteAsync(It.IsAny<long>(), It.IsAny<long>()))
                .ReturnsAsync(false);
            
            var dto = new AvaliacaoDTO { Estrelas = estrelasInvalidas };

            var ex = await Assert.ThrowsAsync<BusinessException>(() =>
                _sut.AvaliarAsync(COMPRADOR_ID, VENDEDOR_ID, TRANSACAO_ID, dto));
            
            Assert.Equal("Número de estrelas deve estar entre 1 e 5.", ex.Message);
        }
        
        [Fact]
        public async Task CompradorAvaliaVendedor_CriaEAdicionaAvaliacao_E_AtualizaReputacaoVendedor()
        {
            var transacaoValida = CriarTransacaoDeTeste(COMPRADOR_ID, VENDEDOR_ID, EstadoTransacao.CONCLUIDA);
            var dto = new AvaliacaoDTO { Estrelas = 5, Comentario = "Excelente" };
            
            var clienteAvaliado = new Cliente(); 
            
            Avaliacao avaliacaoCapturada = null;

            _transacaoRepo.Setup(r => r.ObterPorIdAsync(TRANSACAO_ID)).ReturnsAsync(transacaoValida);
            _avaliacaoRepo.Setup(r => r.ExisteAsync(TRANSACAO_ID, COMPRADOR_ID)).ReturnsAsync(false);
            _avaliacaoRepo.Setup(r => r.AdicionarAsync(It.IsAny<Avaliacao>()))
                .Callback<Avaliacao>(a => avaliacaoCapturada = a)
                .Returns(Task.CompletedTask);

            _avaliacaoRepo.Setup(r => r.CalcularReputacaoAsync(VENDEDOR_ID))
                .ReturnsAsync((Media: 4.5, Total: 10));
            
            _clienteRepo.Setup(r => r.ObterPorIdAsync(VENDEDOR_ID))
                .ReturnsAsync(clienteAvaliado);
            
            _clienteRepo.Setup(r => r.AtualizarAsync(clienteAvaliado))
                .ReturnsAsync(true);

            var resultado = await _sut.AvaliarAsync(COMPRADOR_ID, VENDEDOR_ID, TRANSACAO_ID, dto);
            
            _avaliacaoRepo.Verify(r => r.AdicionarAsync(It.IsAny<Avaliacao>()), Times.Once);
            Assert.NotNull(resultado);
            Assert.Equal(VENDEDOR_ID, avaliacaoCapturada.AvaliadoId);
            Assert.Equal(5, avaliacaoCapturada.Estrelas);
            
            _avaliacaoRepo.Verify(r => r.CalcularReputacaoAsync(VENDEDOR_ID), Times.Once);
            _clienteRepo.Verify(r => r.ObterPorIdAsync(VENDEDOR_ID), Times.Once);
            _clienteRepo.Verify(r => r.AtualizarAsync(clienteAvaliado), Times.Once);

            Assert.Equal(4.5, clienteAvaliado.ReputacaoMedia);
            Assert.Equal(10, clienteAvaliado.TotalAvaliacoes);
        }
        
        [Fact]
        public async Task VendedorAvaliaComprador_CriaEAdicionaAvaliacao_E_AtualizaReputacaoComprador()
        {
            var transacaoValida = CriarTransacaoDeTeste(COMPRADOR_ID, VENDEDOR_ID, EstadoTransacao.CONCLUIDA);
            var dto = new AvaliacaoDTO { Estrelas = 4, Comentario = "Pagamento rápido" };
            
            var clienteAvaliado = new Cliente();
            
            Avaliacao avaliacaoCapturada = null;

            _transacaoRepo.Setup(r => r.ObterPorIdAsync(TRANSACAO_ID)).ReturnsAsync(transacaoValida);
            _avaliacaoRepo.Setup(r => r.ExisteAsync(TRANSACAO_ID, VENDEDOR_ID)).ReturnsAsync(false); 
            _avaliacaoRepo.Setup(r => r.AdicionarAsync(It.IsAny<Avaliacao>()))
                .Callback<Avaliacao>(a => avaliacaoCapturada = a)
                .Returns(Task.CompletedTask);

            _avaliacaoRepo.Setup(r => r.CalcularReputacaoAsync(COMPRADOR_ID))
                .ReturnsAsync((Media: 4.8, Total: 5));
            
            _clienteRepo.Setup(r => r.ObterPorIdAsync(COMPRADOR_ID))
                .ReturnsAsync(clienteAvaliado);
            
            _clienteRepo.Setup(r => r.AtualizarAsync(clienteAvaliado))
                .ReturnsAsync(true);

            var resultado = await _sut.AvaliarAsync(VENDEDOR_ID, COMPRADOR_ID, TRANSACAO_ID, dto);

            _avaliacaoRepo.Verify(r => r.AdicionarAsync(It.IsAny<Avaliacao>()), Times.Once);
            Assert.NotNull(avaliacaoCapturada);
            Assert.Equal(COMPRADOR_ID, avaliacaoCapturada.AvaliadoId);
            Assert.Equal(4, avaliacaoCapturada.Estrelas);

            _avaliacaoRepo.Verify(r => r.CalcularReputacaoAsync(COMPRADOR_ID), Times.Once);
            _clienteRepo.Verify(r => r.ObterPorIdAsync(COMPRADOR_ID), Times.Once);
            _clienteRepo.Verify(r => r.AtualizarAsync(clienteAvaliado), Times.Once);
            Assert.Equal(4.8, clienteAvaliado.ReputacaoMedia);
            Assert.Equal(5, clienteAvaliado.TotalAvaliacoes);
        }
    }
}
using System;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Interfaces;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BookFlaz.Tests.Application.Services.Integracao
{
    public class ClienteTestes
    {
        private readonly Mock<IClienteRepository> _clienteRepo = new();
        private static IPasswordHasher<Cliente> hasher;
        private static Mock<IEmailService> sender = new();
        private readonly ClienteService _sut;

        public ClienteTestes()
        {
            hasher = new PasswordHasher<Cliente>();
            _sut = new ClienteService(
                _clienteRepo.Object,
                hasher,
                sender.Object
            );
        }

        [Fact]
        public async Task CriarClienteAsync_ComDadosValidos_DeveRetornarTrue()
        {
            var dto = new RegistarClienteDTO
            {
                Nome = "John Doe",
                Email = "johndoe@gmail.com",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("1970-09-12"),
                PasswordHash = "johndoe123"
            };

            _clienteRepo.Setup(c => c.ExisteAsync(dto.Email)).ReturnsAsync(false);
            _clienteRepo.Setup(r => r.GuardarAsync(It.IsAny<Cliente>()))
                .ReturnsAsync(true);

            var cliente = await _sut.CriarUtilizadorAsync(dto);


            Assert.True(cliente.Success);
        }

        [Fact]
        public async Task CriarClienteAsync_ComEmailDuplicado_DeveRetornarFalse()
        {
            var dto1 = new RegistarClienteDTO
            {
                Nome = "John Doe",
                Email = "johndoe@gmail.com",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("1970-09-12"),
                PasswordHash = "johndoe123"
            };

            var dto2 = new RegistarClienteDTO
            {
                Nome = "Pedro Jorge",
                Email = "johndoe@gmail.com",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("1970-09-12"),
                PasswordHash = "pedrojorge123"
            };


            var clienteExistente = dto1.Adapt<Cliente>();
            clienteExistente.SetPasswordHash(
                new PasswordHasher<Cliente>().HashPassword(clienteExistente, dto1.PasswordHash));

            _clienteRepo.SetupSequence(r => r.ObterPorEmailAsync(dto1.Email))
                .ReturnsAsync((Cliente)null)
                .ReturnsAsync(clienteExistente);

            _clienteRepo.Setup(r => r.GuardarAsync(It.IsAny<Cliente>()))
                .ReturnsAsync(true);


            var cliente1 = await _sut.CriarUtilizadorAsync(dto1);
            var cliente2 = await _sut.CriarUtilizadorAsync(dto2);

            Assert.True(cliente1.Success);
            Assert.False(cliente2.Success);
            Assert.Contains("Já existe um utilizador", cliente2.Message);
        }

        [Fact]
        public async Task CriarClienteAsync_ComIdadeMenor18_DeveRetornarFalse()
        {
            var dto = new RegistarClienteDTO
            {
                Nome = "John Doe",
                Email = "johndoe@gmail.com",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("2010-09-12"),
                PasswordHash = "johndoe123"
            };

            var cliente = await _sut.CriarUtilizadorAsync(dto);

            Assert.False(cliente.Success);
        }

        [Fact]
        public async Task CriarClienteAsync_ComEmailInvalido_DeveRetornarFalse()
        {
            var dto = new RegistarClienteDTO
            {
                Nome = "John Doe",
                Email = "johndoe@@gmail.com",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("1970-09-12"),
                PasswordHash = "johndoe123"
            };

            var cliente = await _sut.CriarUtilizadorAsync(dto);

            Assert.False(cliente.Success);
        }

        [Fact]
        public async Task LoginCliente_ComDadosValidos_DeveRetornarTrue()
        {
            string email = "user@example.com";
            string password = "password123";

            var dto = new LoginClienteDTO
            {
                Email = email,
                PasswordHash = password
            };

            var cliente = new Cliente();

            dto.Adapt(cliente);
            cliente.SetPasswordHash(hasher.HashPassword(cliente, password));

            _clienteRepo.Setup(r => r.ObterPorEmailAsync(email)).ReturnsAsync(cliente);

            var login = await _sut.LoginUtilizadorAsync(email, password);

            Assert.True(login.Success);
        }

        [Fact]
        public async Task LoginCliente_ComDadosInvalidos_DeveRetornarTrue()
        {
            string email = "user@example.com";
            string password = "password123";

            var dto = new LoginClienteDTO
            {
                Email = email,
                PasswordHash = password
            };

            var cliente = new Cliente();

            dto.Adapt(cliente);
            cliente.SetPasswordHash(hasher.HashPassword(cliente, "password1234"));

            _clienteRepo.Setup(r => r.ObterPorEmailAsync(email)).ReturnsAsync(cliente);

            var login = await _sut.LoginUtilizadorAsync(email, password);

            Assert.False(login.Success);
        }


        [Fact]
        public async Task LoginCliente_UtilizadorInexistente_DeveRetornarFalse()
        {
            var email = "naoexiste@example.com";
            _clienteRepo.Setup(r => r.ObterPorEmailAsync(email)).ReturnsAsync((Cliente)null);

            var res = await _sut.LoginUtilizadorAsync(email, "qualquer");

            Assert.False(res.Success);
        }

        [Fact]
        public async Task EditarUtilizadorAsync_ClienteInexistente_DeveRetornarFalse()
        {
            var dto = new ClienteDTO
            {
                Nome = "Nome Editado",
                Email = "emaileditado@example.com",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("1980-01-01"),
                PasswordHash = "novapass123"
            };

            _clienteRepo.Setup(r => r.ObterPorIdAsync(1L)).ReturnsAsync((Cliente)null);

            var res = await _sut.EditarUtilizadorAsync(1, dto);

            Assert.False(res.Success);
        }

        [Fact]
        public async Task EditarUtilizadorAsync_AtualizarAsyncFalha_DeveRetornarFalse()
        {
            var dto = new ClienteDTO
            {
                Nome = "Nome Editado",
                Email = "emaileditado@example.com",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("1980-01-01"),
                PasswordHash = "novapass123"
            };

            var existente = new Cliente();

            _clienteRepo.Setup(r => r.ObterPorIdAsync(1L)).ReturnsAsync(existente);

            _clienteRepo.Setup(r => r.AtualizarAsync(It.IsAny<Cliente>())).ReturnsAsync(false);

            var res = await _sut.EditarUtilizadorAsync(1L, dto);

            Assert.False(res.Success);
        }

        [Fact]
        public async Task EditarUtilizadorAsync_DadosValidos_DeveRetornarTrue()
        {
            var dto = new ClienteDTO
            {
                Nome = "Nome Editado",
                Email = "emaileditado@example.com",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("1980-01-01"),
                PasswordHash = "novapass123"
            };

            var existente = new Cliente();

            _clienteRepo.Setup(r => r.ObterPorIdAsync(1L)).ReturnsAsync(existente);
            _clienteRepo.Setup(r => r.AtualizarAsync(It.IsAny<Cliente>())).ReturnsAsync(true);


            var res = await _sut.EditarUtilizadorAsync(1L, dto);

            Assert.True(res.Success);
            Assert.Matches("Cliente atualizado com sucesso.", res.Message);
        }

        [Fact]
        public async Task EliminarUtilizadorAsync_ClienteInexistente_DeveRetornarFalse()
        {
            const long id = 45L;

            _clienteRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((Cliente)null);

            var res = await _sut.EliminarUtilizadorAsync(45);

            Assert.False(res.Success);
            Assert.Matches("Cliente não existe.", res.Message);
        }

        [Fact]
        public async Task EliminarUtilizadorAsync_EliminarAsyncFalha_DeveRetornarFalse()
        {
            const long id = 45L;
            var existente = new Cliente();
            _clienteRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(existente);
            _clienteRepo.Setup(r => r.EliminarAsync(existente)).ReturnsAsync(false);
            var res = await _sut.EliminarUtilizadorAsync(id);
            Assert.False(res.Success);
            Assert.Matches("Erro ao eliminar o cliente.", res.Message);
        }

        [Fact]
        public async Task EliminarUtilizadorAsync_EliminarAsyncSucesso_DeveRetornarTrue()
        {
            const long id = 45L;
            var existente = new Cliente();
            _clienteRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync(existente);
            _clienteRepo.Setup(r => r.AtualizarAsync(existente)).ReturnsAsync(true);
            var res = await _sut.EliminarUtilizadorAsync(id);
            Assert.True(res.Success);
            Assert.Matches("Cliente eliminado com sucesso.", res.Message);

            Assert.False(existente.IsAtivo);

            _clienteRepo.Verify(r => r.AtualizarAsync(existente), Times.Once);

            _clienteRepo.Verify(r => r.EliminarAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact]
        public async Task IsAdminAsync_UserExiste_DeveRetornarValor()
        {
            var existente = new Cliente();
            typeof(Cliente).GetProperty("IsAdmin").SetValue(existente, true);

            _clienteRepo.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(existente);

            var resultado = await _sut.IsAdminAsync(10);

            Assert.Equal(true, resultado);
        }

        [Fact]
        public async Task IsAdminAsync_IdInvalido_DeveRetornarArgumentOutOfRangeException()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.IsAdminAsync(0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.IsAdminAsync(-1));
        }

        [Fact]
        public async Task IsAdminAsync_UserNaoExiste_DeveRetornarNotFoundException()
        {
            _clienteRepo.Setup(r => r.ObterPorIdAsync(99)).ReturnsAsync((Cliente?)null);
            await Assert.ThrowsAsync<NotFoundException>(() => _sut.IsAdminAsync(99));
        }

        [Fact]
        public async Task IsAdminAsync_RepositorioLancaErro_DeveRetornarApplicationException()
        {
            _clienteRepo.Setup(r => r.ObterPorIdAsync(99)).Throws(new InvalidOperationException("Err"));
            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _sut.IsAdminAsync(99));

            Assert.Contains("Erro ao verificar privilégios de administrador.", ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RecebeNotificacaoAsync_UserExiste_DeveRetornarValue(bool pref)
        {
            var c = new Cliente();
            typeof(Cliente).GetProperty("NotificacoesAtivas")?.SetValue(c, pref);
            _clienteRepo.Setup(r => r.ObterPorIdAsync(7)).ReturnsAsync(c);

            var value = await _sut.RecebeNotificacaoAsync(7);

            Assert.Equal(pref, value);
        }

        [Fact]
        public async Task RecebeNotificacaoAsync_IdInvalido_DeveLancarArgumentOutOfRange()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.RecebeNotificacaoAsync(0));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.RecebeNotificacaoAsync(-10));
        }

        [Fact]
        public async Task RecebeNotificacaoAsync_UserInexistente_DeveLancarNotFound()
        {
            _clienteRepo.Setup(r => r.ObterPorIdAsync(404)).ReturnsAsync((Cliente?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _sut.RecebeNotificacaoAsync(404));
        }

        [Fact]
        public async Task RecebeNotificacaoAsync_RepoLancaGenerica_DeveEmbrulharEmApplicationException()
        {
            _clienteRepo.Setup(r => r.ObterPorIdAsync(3)).ThrowsAsync(new Exception("timeout"));

            var ex = await Assert.ThrowsAsync<ApplicationException>(() => _sut.RecebeNotificacaoAsync(3));
            Assert.Contains("Erro ao obter preferência de notificações.", ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AlterarPreferenciaNotificacaoAsync_UserExiste_DeveAlterar(bool pref)
        {
            var c = new Cliente();
            bool estadoInicial = true;
            typeof(Cliente).GetProperty("NotificacoesAtivas")?.SetValue(c, true);

            _clienteRepo.Setup(r => r.ObterPorIdAsync(15)).ReturnsAsync(c);
            _clienteRepo.Setup(r => r.AtualizarAsync(It.IsAny<Cliente>())).ReturnsAsync(true);

            var ok = await _sut.AlterarPreferenciaNotificacaoAsync(15, pref);

            Assert.True(ok);

            if (pref != estadoInicial)
            {
                _clienteRepo.Verify(r => r.AtualizarAsync(It.Is<Cliente>(x =>
                    (bool)(typeof(Cliente).GetProperty("NotificacoesAtivas")!.GetValue(x)!) == pref)), Times.Once);
            }
        }

        [Fact]
        public async Task AlterarPreferenciaNotificacaoAsync_IdInvalido_DeveLancarArgumentOutOfRange()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _sut.AlterarPreferenciaNotificacaoAsync(0, true));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                _sut.AlterarPreferenciaNotificacaoAsync(-10, false));
        }

        [Fact]
        public async Task AlterarPreferenciaNotificacaoAsync_RepoLancaGenerica_DeveEmbrulharEmApplicationException()
        {
            _clienteRepo.Setup(r => r.ObterPorIdAsync(3)).ThrowsAsync(new Exception("timeout"));

            var ex = await Assert.ThrowsAsync<ApplicationException>(() =>
                _sut.AlterarPreferenciaNotificacaoAsync(3, true));
            Assert.Contains("Erro ao alterar a preferência de notificações.", ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PedirResetPasswordAsync_UtilizadorInexistente_RetornaMensagemSeguraSemAcao()
        {
            string userEmail = "naoexiste@exemplo.com";
            _clienteRepo.Setup(r => r.ObterPorEmailAsync(userEmail)).ReturnsAsync((Cliente)null);

            var result = await _sut.PedirResetPasswordAsync(userEmail);

            Assert.True(result.Success);
            Assert.Contains("um link de recuperação foi enviado", result.Message);

            _clienteRepo.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>()), Times.Never);
            sender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task PedirResetPasswordAsync_UtilizadorInativo_RetornaMensagemSeguraSemAcao()
        {
            string userEmail = "inativo@exemplo.com";
            var cliente = new Cliente();
            typeof(Cliente).GetProperty("Email")?.SetValue(cliente, userEmail);
            typeof(Cliente).GetProperty("IsAtivo")?.SetValue(cliente, false);

            _clienteRepo.Setup(r => r.ObterPorEmailAsync(userEmail)).ReturnsAsync(cliente);

            var result = await _sut.PedirResetPasswordAsync(userEmail);

            Assert.True(result.Success);
            Assert.Contains("um link de recuperação foi enviado", result.Message);

            _clienteRepo.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>()), Times.Never);
            sender.Verify(s => s.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
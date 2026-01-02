using System;
using System.Threading.Tasks;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Exceptions;
using BookFlaz.Application.Services;
using BookFlaz.Domain.Entities;
using BookFlaz.Domain.Repositories;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BookFlaz.Tests.Application.Services.Integracao
{
    public class PerfilTestes
    {
        private readonly Mock<IProfileRepository> _profileRepo = new();
        private readonly ProfileService _service;
        private readonly IPasswordHasher<Cliente> _passwordHasher;

        public PerfilTestes()
        {
            _passwordHasher =  new PasswordHasher<Cliente>();
            _service = new ProfileService(
                _profileRepo.Object);
        }

        [Fact]
        public async Task VerPerfil_PerfilInexistente_DeveLancarExcecao()
        {
            const long id = 3;
            _profileRepo.Setup(p => p.ObterPerfilAsync(id)).ReturnsAsync((Cliente)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => _service.VerPerfil(id));
            
            Assert.Equal("Perfil não encontrado.", ex.Message);
        }

        [Fact]
        public async Task VerPerfil_Sucesso_DeveRetornarDTOComNome()
        {
            const long id = 3;

            var dto = new ClienteDTO
            {
                Nome = "John Doe",
                Email = "user@example.com",
                PasswordHash = "password123",
                Telefone = "+351912123123",
                Dob = DateTime.Parse("1980-09-12")
            };

            var cliente = new Cliente();
            dto.Adapt(cliente);
            cliente.SetPasswordHash(_passwordHasher.HashPassword(cliente, cliente.PasswordHash));
            
            
            _profileRepo.Setup(p => p.ObterPerfilAsync(id)).ReturnsAsync(cliente);
            
            var perfil = await _service.VerPerfil(id);
            
            Assert.IsType<VerPerfilDTO>(perfil);
        }
    }
}

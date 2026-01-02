using System.Security.Claims;
using BookFlaz.API.Controllers;
using BookFlaz.Application.DTOs;
using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookFlaz.Tests.API.Controllers;

public class ClienteControllerTests
{
    private readonly Mock<IClienteService> mockService;
    private readonly ClienteController controller;
    private readonly Mock<IIdentityService> identityService;

    public ClienteControllerTests()
    {
        mockService = new Mock<IClienteService>();
        identityService = new Mock<IIdentityService>();
        controller = new ClienteController(mockService.Object, identityService.Object);
    }
    
    /// <summary>
    /// Helper para simular um utilizador autenticado (mockar o token JWT).
    /// </summary>
    private void SetupMockUser(string userId)
    {
        var userClaims = new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(userClaims);
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarOk_QuandoSucesso()
    {
        var dto = new RegistarClienteDTO()
        {
            Nome = "John Doe",
            Email = "johndoe@gmai.com",
            PasswordHash = "password123",
            Telefone = "123456789",
            Dob = DateTime.Now
        };

        mockService.Setup(s => s.CriarUtilizadorAsync(It.IsAny<RegistarClienteDTO>())).ReturnsAsync((true, "Cliente criado com sucesso."));
        var result = await controller.CriarCliente(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Cliente criado com sucesso.", ok.Value);
    }

    [Fact]
    public async Task CriarCliente_DeveRetornarBadRequest_QuandoFalha()
    {
        controller.ModelState.AddModelError("Email", "O email é obrigatório.");

        var dto = new RegistarClienteDTO()
        {
            Nome = "John Doe",
            Email = "johndoe@gmai.com",
            PasswordHash = "password123",
            Telefone = "123456789",
            Dob = DateTime.Now
        };

        mockService.Setup(s => s.CriarUtilizadorAsync(It.IsAny<RegistarClienteDTO>())).ReturnsAsync((true, "Cliente criado com sucesso."));
        var result = await controller.CriarCliente(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async Task LoginCliente_DeveRetornarBadRequest_QuandoFalha()
    {
        controller.ModelState.AddModelError("Email", "O email é obrigatório.");

        var dto = new LoginClienteDTO
        {
            Email = "user@example.com",
            PasswordHash = "password123"
        };

        var result = await controller.LoginCliente(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async Task LoginCliente_Sucesso_DeveRetornarOkComMensagemEToken()
    {
        var dto = new LoginClienteDTO { Email = "user@example.com", PasswordHash = "password123" };

        mockService.Setup(s => s.LoginUtilizadorAsync(dto.Email, dto.PasswordHash))
                          .ReturnsAsync((true, "Login efetuado com sucesso."));
        identityService.Setup(s => s.Authenticate(dto.Email, dto.PasswordHash))
                           .Returns(new Cliente());
        identityService.Setup(s => s.GenerateToken(It.IsAny<Cliente>()))
                           .Returns("fake-token");

        var result = await controller.LoginCliente(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = ok.Value!;

        var message = (string)payload.GetType().GetProperty("Message")!.GetValue(payload)!;
        var token = (string)payload.GetType().GetProperty("Token")!.GetValue(payload)!;

        Assert.Equal("Login efetuado com sucesso.", message);
        Assert.Equal("fake-token", token);
    }

    [Fact]
    public async Task EditarCliente_ModelInvalido_DeveRetornarBadRequest()
    {
        controller.ModelState.AddModelError("Email", "O email é obrigatório");

        var dto = new ClienteDTO
        {
            Nome = "Nome Editado",
            Email = "emaileditado@example.com",
            Telefone = "+351912123123",
            Dob = DateTime.Parse("1980-01-01"),
            PasswordHash = "novapass123"
        };

        const long id = 1L;

        var result = await controller.EditarCliente(id, dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);

    }

    [Fact]
    public async Task EditarCliente_Sucesso_DeveRetornarOkComMensagem()
    {
        var dto = new ClienteDTO
        {
            Nome = "Nome Editado",
            Email = "emaileditado@example.com",
            Telefone = "+351912123123",
            Dob = DateTime.Parse("1980-01-01"),
            PasswordHash = "novapass123"
        };

        const long id = 1L;

        mockService.Setup(r => r.EditarUtilizadorAsync(id, dto)).ReturnsAsync((true, "Cliente atualizado com sucesso."));

        var result = await controller.EditarCliente(id, dto);
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Cliente atualizado com sucesso.", ok.Value);
    }

    [Fact]
    public async Task EliminarCliente_Sucesso_DeveRetornarOkComMensagem()
    {
        const long id = 1L;
        SetupMockUser(id.ToString());

        mockService.Setup(r => r.EliminarUtilizadorAsync(id)).ReturnsAsync((true, "Cliente eliminado com sucesso."));

        var result = await controller.EliminarCliente(id);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Cliente eliminado com sucesso.", okResult.Value);
    }

    [Fact]
    public async Task EliminarCliente_ServicoFalha_DeveRetornarBadRequestComMensagem()
    {
        const long id = 1L;
        SetupMockUser(id.ToString());

        mockService.Setup(r => r.EliminarUtilizadorAsync(id)).ReturnsAsync((false, "Erro ao eliminar o cliente."));

        var result = await controller.EliminarCliente(id);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Erro ao eliminar o cliente.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task EliminarCliente_IdNaoCorrespondeAoToken_RetornaUnauthorized()
    {
        const long idDaRota = 1L;
        const string idDoToken = "2";

        SetupMockUser(idDoToken);

        // Act
        var result = await controller.EliminarCliente(idDaRota);
        
        Assert.IsType<UnauthorizedObjectResult>(result);
            
        mockService.Verify(s => s.EliminarUtilizadorAsync(It.IsAny<long>()), Times.Never);
    }
}
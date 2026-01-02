using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookFlaz.Application.Interfaces;

public interface IClienteService
{
    /// <summary>
    /// Cria um novo utilizador do tipo cliente na plataforma.
    /// </summary>
    /// <param name="dto">Dados necessários para a criação do utilizador.</param>
    /// <returns>Tupla indicando sucesso ou falha da operação e mensagem associada.</returns>
    Task<(bool Success, String Message)> CriarUtilizadorAsync(RegistarClienteDTO client);

    /// <summary>
    /// Faz o login de um utilizador do tipo cliente na plataforma.
    /// </summary>
    /// <param name="email">O email do utilizador</param>
    /// <param name="password">A password do utilizador</param>
    /// <returns>Retorna uma tupla indicando sucesso ou falha da operação e mensagem associada.</returns>
    Task<(bool Success, String Message)> LoginUtilizadorAsync(string email, string password);

    /// <summary>
    /// Edita os dados de um utilizador do tipo cliente na plataforma.
    /// <param name="id">O ID do utilizador a ser editado.</param>
    /// <paramref name="client"/> Os novos dados do utilizador.</param>
    /// <returns>Retorna uma tupla indicando sucesso ou falha da operação e mensagem associada.</returns>
    Task<(bool Success, String Message)> EditarUtilizadorAsync(long id, ClienteDTO client);

    /// <summary>
    /// Elimina um utilizador do tipo cliente da plataforma.
    /// </summary>
    /// <param name="id"> O ID do utilizador a ser eliminado.</param>
    /// <returns>Retorna uma tupla indicando sucesso ou falha da operação e mensagem associada.</returns>
    Task<(bool Success, String Message)> EliminarUtilizadorAsync(long id);

    Task<bool> IsAdminAsync(long idUser);

    Task<bool> RecebeNotificacaoAsync(long idUser);
    Task<bool> AlterarPreferenciaNotificacaoAsync(long id, bool preferencia);

    Task<(bool Success, string Message)> PedirResetPasswordAsync(string email);
    Task<Cliente?> ObterClientePorId(long id);
}
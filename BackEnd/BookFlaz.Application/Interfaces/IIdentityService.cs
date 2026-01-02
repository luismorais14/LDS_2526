using BookFlaz.Application.DTOs;
using BookFlaz.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookFlaz.Application.Interfaces;

public interface IIdentityService
{
    /// <summary>
    /// Method to authenticate a user using email and password.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <returns> Cliente object if authentication is successful; otherwise, null.</returns>
    Cliente Authenticate( string email, string password);
    
    /// <summary>
    /// Method to generate a JWT token for an authenticated user.
    /// </summary>
    /// <param name="user">Authenticated user</param>
    /// <returns> JWT token as a string.</returns>
    String GenerateToken(Cliente user);
}
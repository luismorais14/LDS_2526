using BookFlaz.Application.Interfaces;
using BookFlaz.Domain.Entities;
using BookFlaz.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookFlaz.Application.Exceptions;

namespace BookFlaz.Application.Services;

public class AutenticationService : IIdentityService
{
    private readonly BooksContext _dbContext;
    private readonly IPasswordHasher<Cliente> _hasher;
    private readonly string _tokenSecret;
    private readonly string _issuer;
    private readonly string _audience;

    /// <summary>
    /// Constructor for AutenticationService.
    /// </summary>
    public AutenticationService(BooksContext context, IPasswordHasher<Cliente> hasher, string tokenSecret, string issuer = "BookFlazIssuer", string audience = "BookFlazAudience")
    {
        this._dbContext = context;
        this._hasher = hasher;
        this._tokenSecret = tokenSecret;
        this._issuer = issuer;
        this._audience = audience;
    }

    /// <summary>
    /// Method to authenticatse a user using email and password.
    /// </summary>
    /// <param name="email">User email</param>
    /// <param name="password">User password</param>
    /// <returns>Cliente object if authentication is successful; otherwise, null.</returns>
    public Cliente Authenticate(string email, string password)
    {
        var user = _dbContext.Clientes.FirstOrDefault(c => c.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            throw new NotFoundException("Email e/ou password inválidos.");
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result != PasswordVerificationResult.Success)
        {
            throw new Exception("Erro ao encriptar password.");
        }

        return user;
    }

    /// <summary>
    /// Method to generate a JWT token for an authenticated user.
    /// </summary>
    /// <param name="user">User authenticated</param>
    /// <returns>JWT token as a string.</returns>
    public string GenerateToken(Cliente user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_tokenSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            }),
            Issuer = _issuer,
            Audience = _audience,
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
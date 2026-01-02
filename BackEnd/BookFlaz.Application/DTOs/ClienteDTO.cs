using System.ComponentModel.DataAnnotations;

namespace BookFlaz.Application.DTOs;

public class ClienteDTO
{
    [StringLength(250)]
    public string Nome { get; set; } = string.Empty;
    
    [StringLength(300)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(20)]
    [Phone]
    public string Telefone {get; set; } = string.Empty;
    
    public DateTime Dob { get; set; }
}
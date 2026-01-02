using System.ComponentModel.DataAnnotations;

namespace BookFlaz.Application.DTOs;

public class RegistarClienteDTO
{
    [Required]
    [StringLength(250)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [StringLength(300)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Phone]
    public string Telefone {get; set; } = string.Empty;
    
    [Required]
    public DateTime Dob { get; set; }
}
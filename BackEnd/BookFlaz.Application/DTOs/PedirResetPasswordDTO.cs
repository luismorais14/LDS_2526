using System.ComponentModel.DataAnnotations;

namespace BookFlaz.Application.DTOs;

public class PedirResetPasswordDTO
{
    public PedirResetPasswordDTO() {}
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
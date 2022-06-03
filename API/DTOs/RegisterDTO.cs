using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDTO
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Pseudo { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string Country { get; set; }
    [Required]
    public string Gender { get; set; }
    [Required]
    public DateTime DateOfBirth { get; set; }
    [Required]
    [StringLength(32, MinimumLength = 8)]
    public string Password { get; set; }
}

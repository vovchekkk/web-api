using System.ComponentModel.DataAnnotations;

namespace WebApi.MinimalApi.Models;

public class UpdateUserDto
{
    [Required]
    public string? Login { get; init; }
    
    [Required]
    public string? FirstName { get; init; }
    
    [Required]
    public string? LastName { get; init; }
}
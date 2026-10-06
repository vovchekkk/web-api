namespace WebApi.MinimalApi.Models;

public class UpdateUserDto
{
    public string? Login { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}
namespace ManoVecinaAPI.DTOs.Auth;

public record RegisterRequestDto(
    string Name,
    string Email,
    string Password,
    string Role,
    string? AdminKey
);
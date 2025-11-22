namespace ManoVecinaAPI.DTOs.Auth;

public record LoginRequestDto(
    string Email,
    string Password
);
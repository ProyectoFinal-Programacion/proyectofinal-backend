namespace ManoVecinaAPI.DTOs.Admin;

public record CreateUserByAdminDto(
    string Name,
    string Email,
    string Password,
    string Role
);
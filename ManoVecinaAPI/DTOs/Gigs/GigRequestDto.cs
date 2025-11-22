namespace ManoVecinaAPI.DTOs.Gigs;

public record GigRequestDto(
    string Title,
    string Description,
    decimal Price,
    string Category
);
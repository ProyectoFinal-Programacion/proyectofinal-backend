namespace ManoVecinaAPI.DTOs.Reviews;

public record ReviewRequestDto(
    int OrderId,
    int FromUserId,
    int ToUserId,
    int Rating,
    string Comment
);
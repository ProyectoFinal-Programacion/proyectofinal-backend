namespace ManoVecinaAPI.DTOs.Orders;

public record CreateOrderRequestDto(
    int ClientId,
    int GigId
);
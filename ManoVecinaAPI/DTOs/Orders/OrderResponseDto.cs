namespace ManoVecinaAPI.DTOs.Orders;

public class OrderResponseDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = "";
    public string WorkerName { get; set; } = "";
    public string GigTitle { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
}
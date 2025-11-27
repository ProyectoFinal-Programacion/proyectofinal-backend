namespace ManoVecinaAPI.DTOs.Reviews;

public class ReviewRequestDto
{
    public int OrderId { get; set; }
    public int ToUserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

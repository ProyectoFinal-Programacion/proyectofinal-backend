namespace ManoVecinaAPI.DTOs.Reviews;

public class ReviewResponseDto
{
    public int Id { get; set; }
    public string FromName { get; set; } = "";
    public string ToName { get; set; } = "";
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
}
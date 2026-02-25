namespace ImperialHR.Api.Dtos;

public class CreateRequestDto
{
    // "Annual leave", "Sick leave", "Unpaid leave", "Study leave"
    public string? Type { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public string? Comment { get; set; }
}
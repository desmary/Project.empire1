namespace ImperialHR.Api.Dtos;

public class EmperorDecisionDto
{
    public bool Approve { get; set; }

    // Emperor може уточнити фінальні дати (опціонально)
    public DateTime? FinalFrom { get; set; }
    public DateTime? FinalTo { get; set; }
}
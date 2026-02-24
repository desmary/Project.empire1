// Dtos/EmperorDecisionDto.cs
namespace ImperialHR.Api.Dtos
{
    public class EmperorDecisionDto
    {
        public bool Approve { get; set; }
        public DateTime? FinalFrom { get; set; }
        public DateTime? FinalTo { get; set; }
    }
}

namespace ImperialHR.Api.Dtos
{
    public class EmperorDecisionDto
    {
        public bool Approve { get; set; }

        // nullable, щоб не було 0001-01-01 і 500 коли дати не передали
        public DateTime? FinalFrom { get; set; }
        public DateTime? FinalTo { get; set; }
    }
}

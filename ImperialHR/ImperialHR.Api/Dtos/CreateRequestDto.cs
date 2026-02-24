// Dtos/CreateRequestDto.cs
namespace ImperialHR.Api.Dtos
{
    public class CreateRequestDto
    {
        public string Type { get; set; } = "Annual leave";
        public DateTime From { get; set; }

        public DateTime To { get; set; }
        public string? Comment { get; set; }
    }
}

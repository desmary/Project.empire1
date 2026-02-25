namespace ImperialHR.Api.Models;

public class Request
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? ApproverId { get; set; }
    public Employee? Approver { get; set; }

    public RequestType Type { get; set; }
    public RequestStatus Status { get; set; }

    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}


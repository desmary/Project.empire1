// Models/Request.cs
namespace ImperialHR.Api.Models
{
    public enum RequestType
    {
        AnnualLeave = 0,
        SickLeave = 1,
        UnpaidLeave = 2,
        StudyLeave = 3
    }

    public enum RequestStatus
    {
        Pending = 0,
        ApprovedByLord = 1,
        RejectedByLord = 2,
        ApprovedByEmperor = 3,
        RejectedByEmperor = 4
    }

    public class Request
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int ApproverId { get; set; }
        public Employee? Approver { get; set; }

        public RequestType Type { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public string? Comment { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}


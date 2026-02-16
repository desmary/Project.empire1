namespace ImperialHR.Api.Models
{
    public enum RequestType
    {
        Vacation = 0,
        DayOff = 1,
        Transfer = 2,
        Promotion = 3,
        Dismissal = 4
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

        // nullable щоб не було 0001-01-01
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }
}


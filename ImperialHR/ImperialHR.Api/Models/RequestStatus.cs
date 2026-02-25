namespace ImperialHR.Api.Models;

public enum RequestStatus
{
    Pending = 0,
    ApprovedByLord = 1,
    RejectedByLord = 2,
    ApprovedByEmperor = 3,
    RejectedByEmperor = 4
}
// Models/Employee.cs
using System.ComponentModel.DataAnnotations;

namespace ImperialHR.Api.Models;

public class Employee
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; }

   
    public int? ManagerId { get; set; }
    
    public Employee? Manager { get; set; }
    
    public List<Employee> Subordinates { get; set; } = new();

    
    public List<Request> Requests { get; set; } = new();
    public List<Request> Approvals { get; set; } = new();
}

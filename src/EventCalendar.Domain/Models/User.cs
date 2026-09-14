namespace EventCalendar.Domain.Models;

public class User
{
    public Guid Id { get; }
    
    public string Login { get; set; }
    
    public string PasswordHash { get; set; }
    
    public Role Role { get; set; }
}
namespace Tellma.Backdoor.Models;

public class TenantDatabase
{
    public int DatabaseId { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ServerName { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? PasswordKey { get; set; }
}

namespace Tellma.Backdoor.Models;

public class AppSettings
{
    public List<TellmaInstance> Instances { get; set; } = [];
    public string? SelectedInstanceId { get; set; }
    public string? LastScript { get; set; }
}

namespace Tellma.Backdoor.Models;

public class TellmaInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string AdminConnection { get; set; } = string.Empty;
    public bool SkipConfirmation { get; set; }
    public int BatchSize { get; set; } = 100;

    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Name))
                return Name;

            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(AdminConnection);
                return builder.DataSource;
            }
            catch
            {
                return "(unnamed)";
            }
        }
    }
}

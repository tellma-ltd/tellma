using Microsoft.Data.SqlClient;
using Tellma.Backdoor.ViewModels;

namespace Tellma.Backdoor.Services;

public class ScriptExecutionService
{
    public async Task ExecuteAsync(
        string script,
        IReadOnlyList<TenantDatabaseRow> targets,
        int batchSize,
        IProgress<(int Index, ExecutionStatus Status, string? Error)> progress,
        CancellationToken ct)
    {
        batchSize = Math.Max(batchSize, 1);
        int skip = 0;

        while (!ct.IsCancellationRequested)
        {
            var batch = targets.Skip(skip).Take(batchSize).ToList();
            if (batch.Count == 0)
                break;

            var batchWithIndex = batch.Select((row, i) => (row, index: skip + i)).ToList();
            await Task.WhenAll(batchWithIndex.Select(async item =>
            {
                var (row, index) = item;
                progress.Report((index, ExecutionStatus.Executing, null));

                try
                {
                    await using var conn = new SqlConnection(row.ConnectionString);
                    await conn.OpenAsync(ct);

                    await using var cmd = new SqlCommand(script, conn)
                    {
                        CommandTimeout = 0
                    };

                    await cmd.ExecuteNonQueryAsync(ct);
                    progress.Report((index, ExecutionStatus.Completed, null));
                }
                catch (OperationCanceledException)
                {
                    progress.Report((index, ExecutionStatus.Cancelled, null));
                }
                catch (SqlException ex)
                {
                    progress.Report((index, ExecutionStatus.Failed, "SQL Exception: " + ex.Message));
                }
                catch (Exception ex)
                {
                    progress.Report((index, ExecutionStatus.Failed, ex.Message));
                }
            }));

            batchWithIndex.Clear();

            skip += batchSize;
        }

        // Mark remaining as cancelled if we were cancelled mid-way
        if (ct.IsCancellationRequested)
        {
            for (int i = skip; i < targets.Count; i++)
            {
                if (targets[i].Status == ExecutionStatus.NotStarted)
                {
                    progress.Report((i, ExecutionStatus.Cancelled, null));
                }
            }
        }
    }
}

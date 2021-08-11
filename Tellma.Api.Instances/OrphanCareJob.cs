using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Repository.Admin;

namespace Tellma.Api.Instances
{
    public class OrphanCareJob : BackgroundService
    {
        private readonly AdminRepository _repo;
        private readonly ILogger<OrphanCareJob> _logger;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly InstancesOptions _options;

        public OrphanCareJob(AdminRepository repo, ILogger<OrphanCareJob> logger, InstanceInfoProvider instanceInfo, IOptions<InstancesOptions> options)
        {
            _repo = repo;
            _logger = logger;
            _instanceInfo = instanceInfo;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    // Begin serializable transaction
                    using var trx = TransactionFactory.Serializable(TransactionScopeOption.RequiresNew);

                    // Load a batch of orphans
                    int keepAliveSeconds = _options.InstanceKeepAliveInSeconds;
                    int batchCount = _options.OrphanAdoptionBatchCount;
                    var orphans = await _repo.AdoptOrphans(_instanceInfo.Id, keepAliveSeconds, batchCount, cancellation);

                    // Make them available for processing to all the various background Jobs
                    _instanceInfo.AddNewlyAdoptedOrphans(orphans);

                    trx.Complete();
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {GetType().Name}.");
                }

                await Task.Delay(_options.OrphanAdoptionFrequencyInSeconds * 1000, cancellation);
            }
        }
    }
}

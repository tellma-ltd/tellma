using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Data;

namespace Tellma.Controllers.Jobs
{
    public class OrphanCareJob : BackgroundService
    {
        private readonly AdminRepositoryLite _repo;
        private readonly ILogger<OrphanCareJob> _logger;
        private readonly IServiceProvider _services;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly JobsOptions _options;

        public OrphanCareJob(AdminRepositoryLite repo, ILogger<OrphanCareJob> logger, IServiceProvider services, InstanceInfoProvider instanceInfo, IOptions<JobsOptions> options)
        {
            _repo = repo;
            _logger = logger;
            _services = services;
            _instanceInfo = instanceInfo;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Begin serializable transaction
                    using var trx = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);

                    // Load a batch of orphans
                    var orphans = await _repo.AdoptOrphans(_instanceInfo.Id, _options.InstanceKeepAliveInSeconds, _options.OrphanAdoptionBatchCount, stoppingToken);

                    // Make them available for processing to all the various background Jobs
                    _instanceInfo.AddNewlyAdoptedOrphans(orphans);

                    trx.Complete();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error executing {nameof(OrphanCareJob)} at {DateTimeOffset.Now}: {ex.Message}");
                }

                await Task.Delay(_options.OrphanAdoptionFrequencyInSeconds * 1000, stoppingToken);
            }
        }
    }
}

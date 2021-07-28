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
    /// <summary>
    /// Sends regular heartbeats to the admin database to keep the record of this instance alive.
    /// This prevents orphans adopted by this instance from being adopted again by another
    /// </summary>
    public class HeartbeatJob : BackgroundService
    {
        private readonly AdminRepository _repo;
        private readonly ILogger<HeartbeatJob> _logger;
        private readonly IServiceProvider _services;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly InstancesOptions _options;

        public HeartbeatJob(AdminRepository repo, ILogger<HeartbeatJob> logger, IServiceProvider services, InstanceInfoProvider instanceInfo, IOptions<InstancesOptions> options)
        {
            _repo = repo;
            _logger = logger;
            _services = services;
            _instanceInfo = instanceInfo;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Begin serializable transaction
                    using var trx = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, TransactionScopeAsyncFlowOption.Enabled);

                    await _repo.Heartbeat(_instanceInfo.Id, _options.InstanceKeepAliveInSeconds, stoppingToken);

                    trx.Complete();
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {nameof(HeartbeatJob)}.");
                }

                await Task.Delay(_options.InstanceHeartRateInSeconds * 1000, stoppingToken);
            }
        }
    }
}

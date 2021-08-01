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

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    // Begin serializable transaction
                    using var trx = Transactions.Serializable(TransactionScopeOption.RequiresNew);

                    await _repo.Heartbeat(_instanceInfo.Id, _options.InstanceKeepAliveInSeconds, cancellation);

                    trx.Complete();
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {GetType().Name}.");
                }

                await Task.Delay(_options.InstanceHeartRateInSeconds * 1000, cancellation);
            }
        }
    }
}

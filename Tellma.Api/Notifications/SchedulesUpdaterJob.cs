using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Instances;
using Tellma.Repository.Application;

namespace Tellma.Api.Notifications
{
    public class SchedulesUpdaterJob : BackgroundService
    {
        #region Lifecycle

        private readonly InstanceInfoProvider _instanceInfo;
        private readonly SchedulesCache _cache;
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ILogger<SchedulesUpdaterJob> _logger;

        public SchedulesUpdaterJob(
            InstanceInfoProvider instanceInfo,
            SchedulesCache cache,
            IApplicationRepositoryFactory repoFactory,
            ILogger<SchedulesUpdaterJob> logger)
        {
            _instanceInfo = instanceInfo;
            _cache = cache;
            _repoFactory = repoFactory;
            _logger = logger;
        }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    // Grab a hold of a concrete list of adopted tenantIds at the current moment
                    var tenantIds = _instanceInfo.AdoptedTenantIds;
                    await Task.WhenAll(tenantIds.Select(async tenantId =>
                    {
                        // (1) Retrieve the schedules version
                        // If we fail to retrieve the version, we will refresh the cache to empty it anyways
                        var version = Guid.NewGuid().ToString();
                        try
                        {
                            var repo = _repoFactory.GetRepository(tenantId);
                            version = await repo.SchedulesVersion__Load(cancellation);
                        }
                        catch (TaskCanceledException) { }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error in {GetType().Name} while loading the schedule version for tenant Id = {tenantId}.");
                        }

                        // (2) Use the schedules version to ensure freshness of the cache
                        // This call does not throw an exception
                        var isStale = await _cache.RefreshSchedulesIfStale(tenantId, version, cancellation);
                        if (isStale)
                        {
                            // Stale schedules in the cache => Notify the main job
                            _cache.CancelCurrentToken();
                        }
                    }));
                }
                catch (TaskCanceledException) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {GetType().Name}.");
                }

                // Go to sleep for 1 minute
                await Task.Delay(1000 * 60, cancellation);
            }
        }
    }
}

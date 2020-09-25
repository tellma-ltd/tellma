using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Data;

namespace Tellma.Controllers.Jobs
{
    public class OrphanCareJob : BackgroundService
    {
        private readonly ILogger<OrphanCareJob> _logger;
        private readonly IServiceProvider _services;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly JobsOptions _options;

        public OrphanCareJob(ILogger<OrphanCareJob> logger, IServiceProvider services, InstanceInfoProvider instanceInfo, IOptions<JobsOptions> options)
        {
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
                    using var scope = _services.CreateScope();

                    var repo = scope.ServiceProvider.GetRequiredService<AdminRepository>();
                    var orphans = await repo.AdoptOrphans(_instanceInfo.Id, _options.InstanceKeepAliveInSeconds, _options.OrphanAdoptionBatchCount, stoppingToken);

                    // This makes them available for processing by all the various background Jobs
                    _instanceInfo.AddNewlyAdoptedOrphans(orphans);
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

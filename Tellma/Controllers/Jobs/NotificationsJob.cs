using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Data;

namespace Tellma.Controllers.Jobs
{
    public class NotificationsJob : BackgroundService
    {
        private readonly ILogger<OrphanCareJob> _logger;
        private readonly IServiceProvider _services;
        private readonly InstanceInfoProvider _instanceInfo;
        private readonly JobsOptions _options;

        public NotificationsJob(ILogger<OrphanCareJob> logger, IServiceProvider services, InstanceInfoProvider instanceInfo, IOptions<JobsOptions> options)
        {
            _logger = logger;
            _services = services;
            _instanceInfo = instanceInfo;
            _options = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}

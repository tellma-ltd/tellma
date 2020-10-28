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
    public class SmsJob : BackgroundService
    {
        private readonly SmsQueue _queue;
        private readonly ILogger<SmsJob> _logger;
        private readonly IServiceProvider _services;

        public SmsJob(SmsQueue queue, ILogger<SmsJob> logger, IServiceProvider services)
        {
            _queue = queue;
            _logger = logger;
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                SmsQueueItem smsItem = await _queue.DequeueAsync(stoppingToken);

                try
                {
                    var repo = new ApplicationRepository(_services);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {nameof(SmsJob)}.");
                }
            }
        }
    }
}

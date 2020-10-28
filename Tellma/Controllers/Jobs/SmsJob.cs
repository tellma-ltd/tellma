using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Data;
using Tellma.Services.Sms;

namespace Tellma.Controllers.Jobs
{
    public class SmsJob : BackgroundService
    {
        private readonly SmsQueue _queue;
        private readonly ISmsSender _smsSender;
        private readonly ILogger<SmsJob> _logger;
        private readonly ApplicationLiteRepository _repo;

        public SmsJob(SmsQueue queue, ISmsSender smsSender, ILogger<SmsJob> logger, ApplicationLiteRepository repo)
        {
            _queue = queue;
            _smsSender = smsSender;
            _logger = logger;
            _repo = repo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    SmsMessage sms = await _queue.DequeueAsync(stoppingToken);

                    try
                    {
                        await _smsSender.SendAsync(sms);

                        var msgIds = new List<int>() { sms.MessageId };
                        await _repo.SmsMessages__UpdateStatus(sms.TenantId, msgIds, );
                    } 
                    catch
                    {

                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in {nameof(SmsJob)}.");
                }
            }
        }
    }
}

using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Tellma.Api.Instances;
using Tellma.Model.Application;
using Tellma.Repository.Application;

namespace Tellma.Api.Notifications
{
    public class AutoNotificationsJob : BackgroundService
    {
        #region Lifecycle

        private readonly InstanceInfoProvider _instanceInfo;
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ILogger<AutoNotificationsJob> _logger;

        public AutoNotificationsJob(InstanceInfoProvider instanceInfo, IApplicationRepositoryFactory repoFactory, ILogger<AutoNotificationsJob> logger)
        {
            _instanceInfo = instanceInfo;
            _repoFactory = repoFactory;
            _logger = logger;
        }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                // Grab a hold of a concrete list of adopted tenantIds at the current moment
                var tenantIds = _instanceInfo.AdoptedTenantIds;

                // Go to sleep until the next round
                await Task.Delay(1000, cancellation);
            }
        }


        //private readonly ConcurrentDictionary<int, EmailSchedule> _emailSchedules = new();

        //public async Task UpdateSchedulesForTenant(int tenantId)
        //{

        //}
    }

    public class AutoScheduleUpdaterJob : BackgroundService
    {
        #region Lifecycle

        private readonly InstanceInfoProvider _instanceInfo;
        private readonly IApplicationRepositoryFactory _repoFactory;
        private readonly ILogger<AutoNotificationsJob> _logger;

        public AutoScheduleUpdaterJob(InstanceInfoProvider instanceInfo, IApplicationRepositoryFactory repoFactory, ILogger<AutoNotificationsJob> logger)
        {
            _instanceInfo = instanceInfo;
            _repoFactory = repoFactory;
            _logger = logger;
        }

        #endregion

        protected override async Task ExecuteAsync(CancellationToken cancellation)
        {
            _logger.LogInformation(GetType().Name + " Started.");

            while (!cancellation.IsCancellationRequested)
            {
                // Grab a hold of a concrete list of adopted tenantIds at the current moment
                var tenantIds = _instanceInfo.AdoptedTenantIds;

                //Task.WhenAll(tenantIds);

                // Go to sleep until the next round
                await Task.Delay(1000, cancellation);
            }
        }
    }

    public class EmailSchedule
    {
        public int TenantId { get; set; }
        public int TemplateId { get; set; }
        public string Expressions { get; set; }
        public IEnumerable<CronExpression> CronExpressions { get; set; }

        /// <summary>
        /// The time when this template was last run.
        /// </summary>
        public DateTimeOffset LastExecuted { get; set; }
    }
}

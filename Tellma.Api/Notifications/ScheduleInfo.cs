using Cronos;
using System;
using System.Collections.Generic;
using System.Linq;
using Tellma.Utilities.Common;

namespace Tellma.Api.Notifications
{
    public class ScheduleInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleInfo"/> class.
        /// </summary>
        public ScheduleInfo(
            int tenantId,
            ScheduleChannel channel,
            int templateId,
            string version,
            IEnumerable<CronExpression> crons,
            DateTimeOffset lastExecuted,
            bool isError)
        {
            TenantId = tenantId;
            Channel = channel;
            TemplateId = templateId;
            Version = version;
            Crons = crons?.ToList();
            LastExecuted = lastExecuted;
            IsError = isError;
        }

        /// <summary>
        /// The tenant Id to which this schedule belongs.
        /// </summary>
        public int TenantId { get; }

        /// <summary>
        /// Email or SMS.
        /// </summary>
        public ScheduleChannel Channel { get; }

        /// <summary>
        /// The Id of the template.
        /// </summary>
        public int TemplateId { get; }

        /// <summary>
        /// The version of the current schedules
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// The CRON expression that determines when to run this notification template.
        /// </summary>
        public IEnumerable<CronExpression> Crons { get; }

        /// <summary>
        /// The time when this template was last run.
        /// </summary>
        public DateTimeOffset LastExecuted { get; set; }

        /// <summary>
        /// True if the last run of this template resulted in a <see cref="ReportableException"/>.
        /// </summary>
        public bool IsError { get; set; }
    }
}

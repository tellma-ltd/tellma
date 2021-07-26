using Microsoft.Extensions.DependencyInjection;
using System;
using Tellma.Services.EmailLogger;

namespace Microsoft.Extensions.Logging
{
    public static class _EmailLoggerExtensions
    {
        public static ILoggingBuilder AddEmailLogger(this ILoggingBuilder builder, Action<EmailLoggerOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, EmailLoggerProvider>();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}

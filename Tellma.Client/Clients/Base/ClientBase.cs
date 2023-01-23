using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tellma.Client
{
    /// <summary>
    /// Base class for all controller clients.
    /// </summary>
    public abstract class ClientBase
    {
        private readonly IClientBehavior _behavior;

        internal ClientBase(IClientBehavior behavior)
        {
            _behavior = behavior;
        }

        protected virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage msg, Request req, CancellationToken cancellation = default)
            => await _behavior.SendAsync(msg, req, cancellation);

        protected abstract string ControllerPath { get; }

        private IEnumerable<string> GetBaseUrlSteps()
        {
            foreach (var step in _behavior.GetBaseUrlSteps())
            {
                yield return step;
            }

            yield return ControllerPath;
        }

        protected UriBuilder GetActionUrlBuilder(params string[] actionPath)
        {
            var steps = GetBaseUrlSteps().Concat(actionPath);
            var url = string.Join('/', steps);

            return new UriBuilder(url);
        }
    }
}

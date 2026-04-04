using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class AgentDefinitionsClient : CrudClientBase<AgentDefinitionForSave, AgentDefinition, int>
    {
        internal AgentDefinitionsClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "agent-definitions";

        public async Task<EntitiesResult<AgentDefinition>> UpdateState(List<int> ids, Request<UpdateStateArguments> request, CancellationToken cancellation = default)
            => await PutAction("update-state", ids, request, AddUpdateStateArgumentsToUrl, cancellation);

        private void AddUpdateStateArgumentsToUrl(UriBuilder uri, UpdateStateArguments args)
        {
            args ??= new UpdateStateArguments();
            AddActionArgumentsToUrl(uri, args);
            uri.AddQueryParameter(nameof(args.State), args.State);
        }
    }
}

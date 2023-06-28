using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using System;

namespace Tellma.Client
{
    public class DocumentsClient : CrudClientBase<DocumentForSave, Document, int>
    {
        private readonly int _definitionId;

        internal DocumentsClient(int definitionId, IClientBehavior behavior) : base(behavior)
        {
            _definitionId = definitionId;
        }

        protected override string ControllerPath => $"documents/{_definitionId}";

        public async Task<EntitiesResult<Document>> Open(List<int> ids, Request<ActionArguments> request = null, CancellationToken cancellation = default)
            => await PutAction("open", ids, request, AddActionArgumentsToUrl, cancellation);

        public async Task<EntitiesResult<Document>> Close(List<int> ids, Request<ActionArguments> request = null, CancellationToken cancellation = default)
            => await PutAction("close", ids, request, AddActionArgumentsToUrl, cancellation);

        public async Task<EntitiesResult<Document>> Cancel(List<int> ids, Request<ActionArguments> request = null, CancellationToken cancellation = default)
            => await PutAction("cancel", ids, request, AddActionArgumentsToUrl, cancellation);

        public async Task<EntitiesResult<Document>> Uncancel(List<int> ids, Request<ActionArguments> request = null, CancellationToken cancellation = default)
            => await PutAction("uncancel", ids, request, AddActionArgumentsToUrl, cancellation);

        public async Task<EntitiesResult<Document>> Assign(List<int> ids, Request<AssignArguments> request, CancellationToken cancellation = default)
            => await PutAction("assign", ids, request, AddAssignArgumentsToUrl, cancellation);

        protected void AddAssignArgumentsToUrl(UriBuilder uri, AssignArguments args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            AddActionArgumentsToUrl(uri, args);
            uri.AddQueryParameter(nameof(args.AssigneeId), args.AssigneeId + "");
            uri.AddQueryParameter(nameof(args.Comment), args.Comment);

        }
    }
}

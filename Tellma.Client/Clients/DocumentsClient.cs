using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

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

        public async Task<EntityResult<Document>> UpdateAssignment(Request<UpdateAssignmentArguments> request, CancellationToken cancellation = default)
        {
            var args = request?.Arguments ?? throw new ArgumentNullException(nameof(request));

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("update-assignment");
            urlBldr.AddQueryParameter(nameof(args.Id), args.Id.ToString());
            urlBldr.AddQueryParameter(nameof(args.Comment), args.Comment);
            AddActionArgumentsToUrl(urlBldr, args);

            // Prepare the message
            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri);

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<GetByIdResponse<Document>>(cancellation)
                .ConfigureAwait(false);

            var entity = response.Result;
            var relatedEntities = response.RelatedEntities;

            var singleton = new List<Document> { entity };
            Unflatten(singleton, relatedEntities, cancellation);

            return new EntityResult<Document>(entity);
        }

        public async Task<EntitiesResult<Document>> SignLines(List<int> lineIds, Request<SignArguments> request = null, CancellationToken cancellation = default)
        {
            if (lineIds == null || !lineIds.Any())
            {
                return EntitiesResult<Document>.Empty();
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("sign-lines");

            // Add query parameters
            var args = request?.Arguments;
            AddSignArgumentsToUrl(urlBldr, args);

            // Prepare the message
            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri)
            {
                Content = ToJsonContent(lineIds)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<Document>>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result?.ToList();
            var relatedEntities = response.RelatedEntities;

            Unflatten(entities, relatedEntities, cancellation);

            return new EntitiesResult<Document>(entities, entities?.Count);
        }

        public async Task<EntitiesResult<Document>> UnsignLines(List<int> signatureIds, Request<ActionArguments> request = null, CancellationToken cancellation = default)
        {
            if (signatureIds == null || !signatureIds.Any())
            {
                return EntitiesResult<Document>.Empty();
            }

            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("unsign-lines");

            // Add query parameters
            var args = request?.Arguments;
            AddActionArgumentsToUrl(urlBldr, args);

            // Prepare the message
            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri)
            {
                Content = ToJsonContent(signatureIds)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<Document>>(cancellation)
                .ConfigureAwait(false);

            var entities = response.Result?.ToList();
            var relatedEntities = response.RelatedEntities;

            Unflatten(entities, relatedEntities, cancellation);

            return new EntitiesResult<Document>(entities, entities?.Count);
        }

        public async Task<List<LineForSave>> AutoGenerateLines(int lineDefId, List<DocumentForSave> entities, Dictionary<string, string> args = null, Request request = null, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("generate-lines", lineDefId.ToString());

            // Add query parameters
            if (args != null)
            {
                foreach (var kvp in args)
                {
                    urlBldr.AddQueryParameter(kvp.Key, kvp.Value);
                }
            }

            // Prepare the message
            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri)
            {
                Content = ToJsonContent(entities)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<LineForSave>>(cancellation)
                .ConfigureAwait(false);

            return response.Result?.ToList();
        }

        public async Task<List<LineForSave>> AutoGenerateLines(List<int> lineDefIds, List<DocumentForSave> entities, Request request = null, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("generate-lines");

            // Add query parameters
            if (lineDefIds != null)
            {
                foreach (var id in lineDefIds)
                {
                    urlBldr.AddQueryParameter("i", id.ToString());
                }
            }

            // Prepare the message
            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri)
            {
                Content = ToJsonContent(entities)
            };

            // Send the message
            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            var response = await httpResponse.Content
                .ReadAsAsync<EntitiesResponse<LineForSave>>(cancellation)
                .ConfigureAwait(false);

            return response.Result?.ToList();
        }

        public async Task<Stream> GetAttachment(int id, int attachmentId, Request request = null, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder(id.ToString(), "attachments", attachmentId.ToString());

            // Prepare the message
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            // Send the message
            var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            return await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public async Task<Stream> CsvTemplateForLines(int lineDefId, Request request = null, CancellationToken cancellation = default)
        {
            // Prepare the URL
            var urlBldr = GetActionUrlBuilder("lines", lineDefId.ToString(), "template");

            // Prepare the message
            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            // Send the message
            var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            // Extract the response
            return await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        #region Helpers

        private void AddAssignArgumentsToUrl(UriBuilder uri, AssignArguments args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            AddActionArgumentsToUrl(uri, args);
            uri.AddQueryParameter(nameof(args.AssigneeId), args.AssigneeId + "");
            uri.AddQueryParameter(nameof(args.Comment), args.Comment);
        }

        private void AddSignArgumentsToUrl(UriBuilder uri, SignArguments args)
        {
            args ??= new SignArguments();
            AddActionArgumentsToUrl(uri, args);
            uri.AddQueryParameter(nameof(args.ToState), args.ToState.ToString());
            uri.AddQueryParameter(nameof(args.RuleType), args.RuleType);
            uri.AddQueryParameter(nameof(args.ReasonId), args.ReasonId?.ToString());
            uri.AddQueryParameter(nameof(args.ReasonDetails), args.ReasonDetails);
            uri.AddQueryParameter(nameof(args.OnBehalfOfUserId), args.OnBehalfOfUserId?.ToString());
            uri.AddQueryParameter(nameof(args.RoleId), args.RoleId?.ToString());
            uri.AddQueryParameter(nameof(args.SignedAt), args.SignedAt?.ToString("o"));
        }

        #endregion
    }
}

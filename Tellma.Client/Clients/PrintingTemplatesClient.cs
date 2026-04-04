using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Dto;
using Tellma.Model.Application;

namespace Tellma.Client
{
    public class PrintingTemplatesClient : CrudClientBase<PrintingTemplateForSave, PrintingTemplate, int>
    {
        internal PrintingTemplatesClient(IClientBehavior behavior) : base(behavior)
        {
        }

        protected override string ControllerPath => "printing-templates";

        public async Task<Stream> Print(int templateId, Request<PrintEntitiesArguments<int>> request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("print", templateId.ToString());

            var args = request?.Arguments ?? new PrintEntitiesArguments<int>();
            urlBldr.AddQueryParameter(nameof(args.Culture), args.Culture);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.OrderBy), args.OrderBy);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());

            if (args.I != null)
            {
                foreach (var id in args.I)
                {
                    urlBldr.AddQueryParameter(nameof(args.I), id.ToString());
                }
            }

            using var msg = new HttpRequestMessage(HttpMethod.Get, urlBldr.Uri);

            var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public async Task<PrintPreviewResponse> PreviewByFilter(PrintingPreviewTemplate entity, Request<PrintEntitiesArguments<int>> request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("preview-by-filter");

            var args = request?.Arguments ?? new PrintEntitiesArguments<int>();
            AddPrintEntitiesArgsToUrl(urlBldr, args);

            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri)
            {
                Content = ToJsonContent(entity)
            };

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<PrintPreviewResponse>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<PrintPreviewResponse> PreviewById(string id, PrintingPreviewTemplate entity, Request<PrintEntityByIdArguments> request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("preview-by-id", id);

            var args = request?.Arguments ?? new PrintEntityByIdArguments();
            urlBldr.AddQueryParameter(nameof(args.Culture), args.Culture);

            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri)
            {
                Content = ToJsonContent(entity)
            };

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<PrintPreviewResponse>(cancellation)
                .ConfigureAwait(false);
        }

        public async Task<PrintPreviewResponse> Preview(PrintingPreviewTemplate entity, Request<PrintArguments> request = null, CancellationToken cancellation = default)
        {
            var urlBldr = GetActionUrlBuilder("preview");

            var args = request?.Arguments ?? new PrintArguments();
            urlBldr.AddQueryParameter(nameof(args.Culture), args.Culture);

            using var msg = new HttpRequestMessage(HttpMethod.Put, urlBldr.Uri)
            {
                Content = ToJsonContent(entity)
            };

            using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
            await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

            return await httpResponse.Content
                .ReadAsAsync<PrintPreviewResponse>(cancellation)
                .ConfigureAwait(false);
        }

        private void AddPrintEntitiesArgsToUrl(System.UriBuilder urlBldr, PrintEntitiesArguments<int> args)
        {
            urlBldr.AddQueryParameter(nameof(args.Culture), args.Culture);
            urlBldr.AddQueryParameter(nameof(args.Filter), args.Filter);
            urlBldr.AddQueryParameter(nameof(args.OrderBy), args.OrderBy);
            urlBldr.AddQueryParameter(nameof(args.Top), args.Top.ToString());
            urlBldr.AddQueryParameter(nameof(args.Skip), args.Skip.ToString());

            if (args.I != null)
            {
                foreach (var id in args.I)
                {
                    urlBldr.AddQueryParameter(nameof(args.I), id.ToString());
                }
            }
        }
    }
}

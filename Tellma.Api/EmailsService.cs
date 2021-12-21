using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;
using Tellma.Utilities.Blobs;
using Tellma.Utilities.Email;

namespace Tellma.Api
{
    public class EmailsService : FactGetByIdServiceBase<EmailForQuery, int, EntitiesResult<EmailForQuery>, EmailResult>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IBlobService _blobService;

        public EmailsService(ApplicationFactServiceBehavior behavior, FactServiceDependencies deps, IBlobService blobService) : base(deps)
        {
            _behavior = behavior;
            _blobService = blobService;
        }

        protected override string View => "emails";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public EmailsService SetIncludeBody(bool val)
        {
            IncludeBody = val;
            return this;
        }

        private bool IncludeBody { get; set; }
       
        public async Task<FileResult> GetAttachment(int emailId, int attachmentId, CancellationToken cancellation)
        {
            await Initialize(cancellation);

            // This enforces read permissions
            string attachments = nameof(EmailForQuery.Attachments);
            var result = await GetById(emailId, new GetByIdArguments
            {
                Select = $"{attachments}.{nameof(EmailAttachment.ContentBlobId)},{attachments}.{nameof(EmailAttachment.Name)}"
            },
            cancellation);

            // Get the blob name
            var attachment = result.Entity?.Attachments?.FirstOrDefault(att => att.Id == attachmentId);
            if (attachment != null && !string.IsNullOrWhiteSpace(attachment.ContentBlobId))
            {
                try
                {
                    // Get the bytes
                    string blobName = EmailUtil.EmailAttachmentBlobName(attachment.ContentBlobId);
                    var bytes = await _blobService.LoadBlobAsync(_behavior.TenantId, blobName, cancellation);

                    // Get the content type
                    var name = attachment.Name;
                    return new FileResult(bytes, name);
                }
                catch (BlobNotFoundException)
                {
                    throw new NotFoundException<int>(attachmentId);
                }
            }
            else
            {
                throw new NotFoundException<int>(attachmentId);
            }
        }

        protected override Task<EntityQuery<EmailForQuery>> Search(EntityQuery<EmailForQuery> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var toEmail = nameof(EmailForQuery.To);
                var subject = nameof(EmailForQuery.Subject);

                // Prepare the filter string
                var filterString = $"{toEmail} contains '{search}' or {subject} contains '{search}'";

                // Apply the filter
                query = query.Filter(ExpressionFilter.Parse(filterString));
            }

            return Task.FromResult(query);
        }

        protected override Task<EntitiesResult<EmailForQuery>> ToEntitiesResult(List<EmailForQuery> data, int? count = null, CancellationToken cancellation = default)
        {
            var result = new EntitiesResult<EmailForQuery>(data, count);
            return Task.FromResult(result);
        }

        protected override async Task<EmailResult> ToEntityResult(EmailForQuery entity, CancellationToken cancellation = default)
        {
            string body = null;
            if (IncludeBody && !string.IsNullOrWhiteSpace(entity.BodyBlobId))
            {
                var blobName = EmailUtil.EmailBodyBlobName(entity.BodyBlobId);
                var bytes = await _blobService.LoadBlobAsync(_behavior.TenantId, blobName, cancellation);
                body = Encoding.UTF8.GetString(bytes);
            }

            return new EmailResult(entity, body);
        }
    }
}

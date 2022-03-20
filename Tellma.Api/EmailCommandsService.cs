using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Application;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class EmailCommandsService : FactGetByIdServiceBase<EmailCommand, int>
    {
        private readonly ApplicationFactServiceBehavior _behavior;
        private readonly IPermissionsCache _permissionsCache;

        public EmailCommandsService(ApplicationFactServiceBehavior behavior, IPermissionsCache permissionsCache, FactServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
            _permissionsCache = permissionsCache;
        }

        protected override string View => "email-commands";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        protected override Task<EntityQuery<EmailCommand>> Search(EntityQuery<EmailCommand> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                // Prepare the filter string
                var caption = nameof(EmailCommand.Caption);
                var filterString = $"{caption} contains '{search}'";

                // Apply the filter
                query = query.Filter(filterString);
            }

            return Task.FromResult(query);
        }

        protected override async Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            // (1) Get the template-specific permissions

            // Get all permissions pertaining to email commands
            string prefix = $"{View}/";
            var templateSpecificPermissions = (await _permissionsCache
                .GenericPermissionsFromCache(
                    tenantId: _behavior.TenantId,
                    userId: UserId,
                    version: _behavior.PermissionsVersion,
                    viewPrefix: prefix,
                    action: action,
                    cancellation: cancellation)).ToList();

            // Massage the permissions by adding definitionId = definitionId as an extra clause 
            // (since the controller will not filter the results per any specific definition Id)
            foreach (var permission in templateSpecificPermissions.Where(e => e.View != "all"))
            {
                string templateIdString = permission.View.Remove(0, prefix.Length);
                if (!int.TryParse(templateIdString, out int templateId))
                {
                    throw new ServiceException($"Could not parse template Id '{templateIdString}' to a valid integer.");
                }

                string definitionPredicate = $"{nameof(EmailCommand.TemplateId)} eq {templateId}";
                if (!string.IsNullOrWhiteSpace(permission.Criteria))
                {
                    permission.Criteria = $"{definitionPredicate} and ({permission.Criteria})";
                }
                else
                {
                    permission.Criteria = definitionPredicate;
                }
            }

            // (2) Get the generic permissions
            var genericPermissions = await base.UserPermissions(action, cancellation);

            // Return
            return templateSpecificPermissions.Concat(genericPermissions);
        }
    }
}

using Tellma.Controllers.Dto;
using Tellma.Controllers.Utilities;
using Tellma.Data;
using Tellma.Data.Queries;
using Tellma.Model.Application;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Tellma.Controllers
{
    [Route("api/" + BASE_ADDRESS)]
    [ApplicationController]
    public class AccountsController : CrudControllerBase<AccountForSave, Account, int>
    {
        public const string BASE_ADDRESS = "accounts";

        private readonly AccountsService _service;

        public AccountsController(AccountsService service, IServiceProvider sp) : base(sp)
        {
            _service = service;
        }

        [HttpPut("activate")]
        public async Task<ActionResult<EntitiesResponse<Account>>> Activate([FromBody] List<int> ids, [FromQuery] ActivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Activate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, _logger);
        }

        [HttpPut("deactivate")]
        public async Task<ActionResult<EntitiesResponse<Account>>> Deactivate([FromBody] List<int> ids, [FromQuery] DeactivateArguments args)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                var serverTime = DateTimeOffset.UtcNow;
                var (data, extras) = await _service.Deactivate(ids: ids, args);
                var response = TransformToEntitiesResponse(data, extras, serverTime, cancellation: default);
                return Ok(response);
            }, _logger);
        }

        protected override CrudServiceBase<AccountForSave, Account, int> GetCrudService()
        {
            return _service;
        }
    }

    public class AccountsService : CrudServiceBase<AccountForSave, Account, int>
    {
        private static readonly string _documentDetailsSelect = string.Join(',', DocDetails.AccountPaths());

        private readonly ApplicationRepository _repo;

        private string View => AccountsController.BASE_ADDRESS;

        public AccountsService(ApplicationRepository repo, IServiceProvider sp) : base(sp)
        {
            _repo = repo;
        }

        protected override Task<IEnumerable<AbstractPermission>> UserPermissions(string action, CancellationToken cancellation)
        {
            return _repo.PermissionsFromCache(View, action, cancellation);
        }

        protected override IRepository GetRepository()
        {
            return _repo;
        }

        protected override Query<Account> Search(Query<Account> query, GetArguments args)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var name = nameof(Account.Name);
                var name2 = nameof(Account.Name2);
                var name3 = nameof(Account.Name3);
                var code = nameof(Account.Code);

                query = query.Filter($"{name} contains '{search}' or {name2} contains '{search}' or {name3} contains '{search}' or {code} contains '{search}'");
            }

            return query;
        }

        protected override async Task<List<AccountForSave>> SavePreprocessAsync(List<AccountForSave> entities)
        {
            // Defaults
            entities.ForEach(entity =>
            {
                // Can't have a relation without the relation definition
                if (entity.RelationDefinitionId == null)
                {
                    entity.RelationId = null;
                }

                // Can't have a resource without the resource definition
                if (entity.ResourceDefinitionId == null)
                {
                    entity.ResourceId = null;
                }

                // Can't have a noted relation without the noted relation definition
                if (entity.NotedRelationDefinitionId == null)
                {
                    entity.NotedRelationId = null;
                }
            });

            // SQL Preprocessing
            await _repo.Accounts__Preprocess(entities);
            return entities;
        }

        protected override async Task SaveValidateAsync(List<AccountForSave> entities)
        {
            //foreach (var (entity, index) in entities.Select((e, i) => (e, i)))
            //{
            //    if (ModelState.HasReachedMaxErrors)
            //    {
            //        // No need to keep going forever
            //        break;
            //    }
            //}

            //// No need to invoke SQL if the model state is full of errors
            //if (ModelState.HasReachedMaxErrors)
            //{
            //    // null Ids will cause an error when calling the SQL validation
            //    return;
            //}

            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Accounts_Validate__Save(entities, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<AccountForSave> entities, bool returnIds)
        {
            return await _repo.Accounts__Save(entities, returnIds: returnIds);
        }

        protected override async Task DeleteValidateAsync(List<int> ids)
        {
            // SQL validation
            int remainingErrorCount = ModelState.MaxAllowedErrors - ModelState.ErrorCount;
            var sqlErrors = await _repo.Accounts_Validate__Delete(ids, top: remainingErrorCount);

            // Add errors to model state
            ModelState.AddLocalizedErrors(sqlErrors, _localizer);
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            try
            {
                await _repo.Accounts__Delete(ids);
            }
            catch (ForeignKeyViolationException)
            {
                throw new BadRequestException(_localizer["Error_CannotDelete0AlreadyInUse", _localizer["Account"]]);
            }
        }

        protected override ExpressionOrderBy DefaultOrderBy()
        {
            return ExpressionOrderBy.Parse(nameof(Account.Code));
        }

        protected override ExpressionSelect ParseSelect(string select)
        {
            string shorthand = "$DocumentDetails";
            if (select == null)
            {
                return null;
            }
            else
            {
                select = select.Replace(shorthand, _documentDetailsSelect);
                return base.ParseSelect(select);
            }
        }

        public Task<(List<Account>, Extras)> Activate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: true);
        }

        public Task<(List<Account>, Extras)> Deactivate(List<int> ids, ActionArguments args)
        {
            return SetIsActive(ids, args, isActive: false);
        }

        private async Task<(List<Account>, Extras)> SetIsActive(List<int> ids, ActionArguments args, bool isActive)
        {
            // Check user permissions
            var action = "IsActive";
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            ids = await CheckActionPermissionsBefore(actionFilter, ids);

            // Execute and return
            using var trx = ControllerUtilities.CreateTransaction();
            await _repo.Accounts__Activate(ids, isActive);

            List<Account> data = null;
            Extras extras = null;

            if (args.ReturnEntities ?? false)
            {
                (data, extras) = await GetByIds(ids, args, action, cancellation: default);
            }

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, ids, data);

            trx.Complete();
            return (data, extras);
        }
    }
}

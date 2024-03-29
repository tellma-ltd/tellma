﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Base;
using Tellma.Api.Behaviors;
using Tellma.Api.Dto;
using Tellma.Model.Admin;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api
{
    public class IdentityServerClientsService : CrudServiceBase<IdentityServerClientForSave, IdentityServerClient, int>
    {
        private readonly AdminFactServiceBehavior _behavior;

        protected override string View => "identity-server-clients";

        protected override IFactServiceBehavior FactBehavior => _behavior;

        public IdentityServerClientsService(
            AdminFactServiceBehavior behavior,
            CrudServiceDependencies deps) : base(deps)
        {
            _behavior = behavior;
        }

        protected override Task<EntityQuery<IdentityServerClient>> Search(EntityQuery<IdentityServerClient> query, GetArguments args, CancellationToken _)
        {
            string search = args.Search;
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Replace("'", "''"); // escape quotes by repeating them

                var memo = nameof(IdentityServerClient.Memo);
                var name = nameof(IdentityServerClient.Name);
                var clientId = nameof(IdentityServerClient.ClientId);

                query = query.Filter($"{name} contains '{search}' or {memo} contains '{search}' or {clientId} eq '{search}'");
            }

            return Task.FromResult(query);
        }

        protected override Task<List<IdentityServerClientForSave>> SavePreprocessAsync(List<IdentityServerClientForSave> entities)
        {
            // Generate client Id and client Secret for all new clients
            entities.ForEach(e =>
            {
                if (e.Id == 0)
                {
                    e.ClientId = RandomClientId();
                    e.ClientSecret = CryptographicallyStrongClientSecret();
                }
            });
            return Task.FromResult(entities);
        }

        protected override async Task<List<int>> SaveExecuteAsync(List<IdentityServerClientForSave> entities, bool returnIds)
        {
            // Save clients in the admin database
            var result = await _behavior.Repository.IdentityServerClients__Save(
                    entities: entities,
                    returnIds: returnIds,
                    validateOnly: ModelState.IsError,
                    top: ModelState.RemainingErrors,
                    userId: UserId); // Synchronizes with directory automatically

            AddErrorsAndThrowIfInvalid(result.Errors);

            // Return the new Ids
            return result.Ids;
        }

        protected override async Task DeleteExecuteAsync(List<int> ids)
        {
            var result = await _behavior.Repository.IdentityServerClients__Delete(
                ids: ids,
                validateOnly: ModelState.IsError,
                top: ModelState.RemainingErrors,
                userId: UserId); // Synchronizes with directory automatically

            AddErrorsAndThrowIfInvalid(result.Errors);
        }

        private static string RandomClientId()
        {
            return "m2m-" + Guid.NewGuid().ToString("D");
        }

        private static string CryptographicallyStrongClientSecret()
        {
            // Generate a sequence of cryptographically strong random bytes
            using var provider = new RNGCryptoServiceProvider();
            var secretBytes = new byte[32];
            provider.GetBytes(secretBytes); // Fills the byte array
            
            // Convert the bytes to a string
            StringBuilder bldr = new();
            foreach (var b in secretBytes)
            {
                bldr.Append(b.ToString("x2"));
            }

            return bldr.ToString();
        }

        public async Task<EntitiesResult<IdentityServerClient>> ResetClientSecret(ResetClientSecretArguments args)
        {
            await Initialize();
            
            // Check permissions
            var idSingleton = new List<int> { args.Id }; // A single Id
            var action = PermissionActions.Update;
            var actionFilter = await UserPermissionsFilter(action, cancellation: default);
            idSingleton = await CheckActionPermissionsBefore(actionFilter, idSingleton);

            // Invisible or missing user
            if (!idSingleton.Any())
            {
                // The user cannot view that user, we pretend it doesn't exist
                throw new NotFoundException<int>(args.Id);
            }

            // Reset the secret
            var newSecret = CryptographicallyStrongClientSecret();
            using var trx = TransactionFactory.ReadCommitted();
            await _behavior.Repository.IdentityServerClients__UpdateSecret(args.Id, newSecret, UserId);


            var result = (args.ReturnEntities ?? false) ?
                await GetByIds(idSingleton, args, action, cancellation: default) :
                EntitiesResult<IdentityServerClient>.Empty();

            // Check user permissions again
            await CheckActionPermissionsAfter(actionFilter, idSingleton, result.Data);

            trx.Complete();
            return result;
        }
    }
}

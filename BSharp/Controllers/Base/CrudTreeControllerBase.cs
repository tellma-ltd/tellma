using BSharp.Controllers.Utilities;
using BSharp.Entities;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{
    /// <summary>
    /// Controllers inheriting from this class allow searching, aggregating and exporting a certain
    /// entity type that inherits from <see cref="EntityWithKey{TKey}"/> using OData-like parameters
    /// and allow selecting a certain record by Id, as well as updating, deleting, deleting with descendants
    /// and importing lists of that entity
    /// </summary>
    public abstract class CrudTreeControllerBase<TEntityForSave, TEntity, TKey> : CrudControllerBase<TEntityForSave, TEntity, TKey>
        where TEntityForSave : EntityWithKey<TKey>, new()
        where TEntity : EntityWithKey<TKey>, new()
    {
        private readonly ILogger _logger;

        public CrudTreeControllerBase(ILogger logger, IStringLocalizer localizer) : base(logger, localizer)
        {
            _logger = logger;
        }

        [HttpDelete("with-descendants")]
        public virtual async Task<ActionResult> DeleteWithDescendants([FromBody] List<TKey> ids)
        {
            return await ControllerUtilities.InvokeActionImpl(async () =>
            {
                await DeleteWithDescendantsImplAsync(ids);
                return Ok();
            }, _logger);
        }

        /// <summary>
        /// Assumes that the view does not allow 'Create' permission level, if it does
        /// need to override it
        /// </summary>
        protected virtual async Task DeleteWithDescendantsImplAsync(List<TKey> ids)
        {
            if (ids == null || !ids.Any())
            {
                return;
            }

            await CheckActionPermissions(Constants.Delete, ids.ToArray());
            await ValidateDeleteWithDescendantsAsync(ids);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            await DeleteWithDescendantsAsync(ids);
        }

        /// <summary>
        /// Deletes the entities specified by the list of Ids
        /// </summary>
        protected abstract Task DeleteWithDescendantsAsync(List<TKey> ids);

        /// <summary>
        /// Validates the delete operation before it happens
        /// </summary>
        protected virtual Task ValidateDeleteWithDescendantsAsync(List<TKey> ids)
        {
            return Task.CompletedTask;
        }
    }
}

using BSharp.Controllers.DTO;
using BSharp.Controllers.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers
{

    public abstract class CrudTreeControllerBase<TDtoForSave, TDto, TDtoForQuery, TKey> : CrudControllerBase<TDtoForSave, TDto, TDtoForQuery, TKey>
        where TDtoForQuery : DtoForSaveKeyBase<TKey>, new()
        where TDtoForSave : DtoForSaveKeyBase<TKey>, new()
        where TDto : DtoForSaveKeyBase<TKey>, new()
    {
        private readonly ILogger _logger;

        public CrudTreeControllerBase(ILogger logger, IStringLocalizer localizer, IServiceProvider serviceProvider) : base(logger, localizer, serviceProvider)
        {
            _logger = logger;
        }

        [HttpDelete("with-descendants")]
        public virtual async Task<ActionResult> DeleteWithDescendants([FromBody] List<TKey> ids)
        {
            return await ControllerUtilities.ExecuteAndHandleErrorsAsync(async () =>
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

            await CheckActionPermissions(ids);
            await ValidateDeleteWithDescendantsAsync(ids);
            if (!ModelState.IsValid)
            {
                throw new UnprocessableEntityException(ModelState);
            }

            await DeleteWithDescendantsAsync(ids);
        }

        /// <summary>
        /// Deletes the entities specified by the list of Ids
        /// Assumes that the view does not allow 'Create' permission level, if it does
        /// ignore this method and override <see cref="DeleteImplAsync(List{TKey})"/> instead
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

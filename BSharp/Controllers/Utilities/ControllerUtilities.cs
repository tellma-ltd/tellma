using BSharp.Controllers.Dto;
using BSharp.Data;
using BSharp.EntityModel;
using BSharp.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;

namespace BSharp.Controllers.Misc
{
    public static class ControllerUtilities
    {
        public const string ALL = "all";

        /// <summary>
        /// Calls the provided function and handles the special exceptions by turning them into <see cref="ActionResult"/>s.
        /// Action implementations can then throw these exceptions when there is an error, making the implementatio neasier
        /// </summary>
        public static async Task<ActionResult<T>> InvokeActionImpl<T>(Func<Task<ActionResult<T>>> func, ILogger logger)
        {
            try
            {
                return await func();
            }
            catch (ForbiddenException)
            {
                return new StatusCodeResult(403);
            }
            catch (NotFoundException<int?> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<int> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<string> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (UnprocessableEntityException ex)
            {
                return new UnprocessableEntityObjectResult(ex.ModelState);
            }
            catch (BadRequestException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public static async Task<ActionResult> InvokeActionImpl(Func<Task<ActionResult>> func, ILogger logger)
        {
            try
            {
                return await func();
            }
            catch (ForbiddenException)
            {
                return new StatusCodeResult(403);
            }
            catch (NotFoundException<int?> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<int> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (NotFoundException<string> ex)
            {
                return new NotFoundObjectResult(ex.Ids);
            }
            catch (UnprocessableEntityException ex)
            {
                return new UnprocessableEntityObjectResult(ex.ModelState);
            }
            catch (BadRequestException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error: {ex.Message} {ex.StackTrace}");
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// If some 2 or more entities have the same Id that isn't 0, an appropriate error is added to the <see cref="ModelStateDictionary"/>
        /// </summary>
        public static void ValidateUniqueIds<TEntity>(List<TEntity> entities, ModelStateDictionary modelState, IStringLocalizer localizer) where TEntity : EntityWithKey
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (modelState is null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (localizer is null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }

            // Check that Ids are unique
            var duplicateIds = entities.Where(e => !e.GetId().Equals(0)).GroupBy(e => e.GetId()).Where(g => g.Count() > 1);
            if(duplicateIds.Any())
            {
                // Hash the entities' indices for performance
                Dictionary<TEntity, int> indices = entities.ToIndexDictionary();

                foreach (var groupWithDuplicateIds in duplicateIds)
                {
                    foreach (var entity in groupWithDuplicateIds)
                    {
                        // This error indicates a bug
                        var index = indices[entity];
                        modelState.AddModelError($"[{index}].Id", localizer["Error_TheEntityWithId0IsSpecifiedMoreThanOnce", entity.GetId()]);
                    }
                }
            }
        }

        /// <summary>
        /// SQL validation may return error message names (for localization) as well as some arguments 
        /// this method parses those arguments into objects based on their prefix for example date:2019-01-13
        /// will be parsed to datetime object suitable for formatting in C# into the error message
        /// </summary>
        public static object[] ToFormatArguments(this ValidationError @this)
        {
            object Parse(string str)
            {
                // TODO Implement properly
                if (string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }

                if (DateTime.TryParse(str, out DateTime dResult))
                {
                    return dResult;
                }

                return str;
            }

            object[] formatArguments = {
                    Parse(@this.Argument1),
                    Parse(@this.Argument2),
                    Parse(@this.Argument3),
                    Parse(@this.Argument4),
                    Parse(@this.Argument5)
                };

            return formatArguments;
        }

        /// <summary>
        /// The method localizes every error in the collection and adds it to the <see cref="ModelStateDictionary"/>
        /// </summary>
        public static void AddLocalizedErrors(this ModelStateDictionary modelState, IEnumerable<ValidationError> errors, IStringLocalizer localizer)
        {
            foreach (var error in errors)
            {
                var formatArguments = error.ToFormatArguments();

                string key = error.Key;
                string errorMessage = localizer[error.ErrorName, formatArguments];

                modelState.AddModelError(key: key, errorMessage: errorMessage);
            }
        }

        /// <summary>
        /// Returns a very common transaction scope which uses <see cref="TransactionScopeOption.Required"/> by default, 
        /// an isolation level of <see cref="System.Data.IsolationLevel.ReadCommitted"/> by default and
        /// <see cref="TransactionScopeAsyncFlowOption.Enabled"/>. Defaults can be overridden with arguments
        /// </summary>
        public static TransactionScope CreateTransaction(TransactionScopeOption? scopeOption = null, TransactionOptions? options = null)
        {
            return new TransactionScope(
                        scopeOption: scopeOption ?? TransactionScopeOption.Required,
                        transactionOptions: options ?? new TransactionOptions
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                            Timeout = DefaultTransactionTimeout()
                        },
                        asyncFlowOption: TransactionScopeAsyncFlowOption.Enabled);
        }

        /// <summary>
        /// Returns the universal default timeout of 5 minutes, if every transaction used this method
        /// it makes it easier to change the timeout when necessary
        /// </summary>
        public static TimeSpan DefaultTransactionTimeout()
        {
            return TimeSpan.FromMinutes(5);
        }
    }
}

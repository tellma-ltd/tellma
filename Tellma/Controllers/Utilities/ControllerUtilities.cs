using Tellma.Data;
using Tellma.Entities;
using Tellma.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Threading;
using System.Reflection;
using Tellma.Entities.Descriptors;

namespace Tellma.Controllers.Utilities
{
    public static class ControllerUtilities
    {
        /// <summary>
        /// Calls the provided function and handles the special exceptions by turning them into <see cref="ActionResult"/>s.
        /// Action implementations can then throw these exceptions when there is an error, making the implementation easier
        /// </summary>
        public static async Task<ActionResult<T>> InvokeActionImpl<T>(Func<Task<ActionResult<T>>> func, ILogger logger)
        {
            try
            {
                return await func();
            }
            catch (TaskCanceledException)
            {
                return new OkResult();
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
            catch (MethodNotAllowedException)
            {
                return new StatusCodeResult(405);
            }
            catch (UnprocessableEntityException ex)
            {
                return new UnprocessableEntityObjectResult(ToModelState(ex.ModelState));
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
        /// Calls the provided function and handles the special exceptions by turning them into <see cref="ActionResult"/>s.
        /// Action implementations can then throw these exceptions when there is an error, making the implementation easier
        /// </summary>
        public static async Task<ActionResult> InvokeActionImpl(Func<Task<ActionResult>> func, ILogger logger)
        {
            try
            {
                return await func();
            }
            catch (TaskCanceledException)
            {
                return new OkResult();
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
            catch (MethodNotAllowedException)
            {
                return new StatusCodeResult(405);
            }
            catch (UnprocessableEntityException ex)
            {
                return new UnprocessableEntityObjectResult(ToModelState(ex.ModelState));
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
        /// Transforms a business logic layer <see cref="ValidationErrorsDictionary"/> to the web controller's <see cref="ModelStateDictionary"/>
        /// </summary>
        public static ModelStateDictionary ToModelState(ValidationErrorsDictionary validationErrors)
        {
            var result = new ModelStateDictionary();
            foreach (var (key, errors) in validationErrors.AllErrors)
            {
                foreach (var error in errors)
                {
                    result.AddModelError(key, error);
                }
            }

            return result;
        }

        /// <summary>
        /// If some 2 or more entities have the same Id that isn't 0, an appropriate error is added to the <see cref="ValidationErrorsDictionary"/>
        /// </summary>
        public static void ValidateUniqueIds<TEntity>(List<TEntity> entities, ValidationErrorsDictionary modelState, IStringLocalizer localizer) where TEntity : EntityWithKey
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
            var duplicateIds = entities.Where(e => !(e.GetId()?.Equals(0) ?? true)) // takes away the nulls too
                .GroupBy(e => e.GetId())
                .Where(g => g.Count() > 1);

            if (duplicateIds.Any())
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
        public static object[] ToFormatArguments(this ValidationError @this, IStringLocalizer localizer)
        {
            static object Parse(string str, IStringLocalizer localizer)
            {
                // Null returns null
                if (string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }

                // Anything with this prefix is translated
                var translateKey = "localize:";
                if (str.StartsWith(translateKey))
                {
                    str = str.Remove(0, translateKey.Length);
                    return localizer[str];
                }

                return str;
            }

            object[] formatArguments = {
                    Parse(@this.Argument1, localizer),
                    Parse(@this.Argument2, localizer),
                    Parse(@this.Argument3, localizer),
                    Parse(@this.Argument4, localizer),
                    Parse(@this.Argument5, localizer)
                };

            return formatArguments;
        }

        /// <summary>
        /// The method localizes every error in the collection and adds it to the <see cref="ValidationErrorsDictionary"/>
        /// </summary>
        public static void AddLocalizedErrors(this ValidationErrorsDictionary modelState, IEnumerable<ValidationError> errors, IStringLocalizer localizer)
        {
            foreach (var error in errors)
            {
                var formatArguments = error.ToFormatArguments(localizer);

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

        /// <summary>
        /// From an <see cref="Expression"/> that accesses the property (e.g. "e => e.Name"),
        /// it computes another <see cref="Expression"/> that sets the property, (e.g. "(e, v) => e.Name = v")
        /// </summary>
        public static Expression<Action<TClass, TProp>> GetAssigner<TClass, TProp>(Expression<Func<TClass, TProp>> propAccessor)
        {
            var prop = ((MemberExpression)propAccessor.Body).Member;
            var typeParam = Expression.Parameter(typeof(TClass));
            var valueParam = Expression.Parameter(typeof(TProp));

            return Expression.Lambda<Action<TClass, TProp>>(
                Expression.Assign(
                    Expression.MakeMemberAccess(typeParam, prop),
                    valueParam), typeParam, valueParam);
        }

        /// <summary>
        /// Takes a list of <see cref="Entity"/>'s, and for every entity it inspects the navigation properties, if a navigation property
        /// contains an <see cref="Entity"/> with a strong type, it sets that property to null, and moves the strong entity into a separate
        /// "relatedEntities" hash set, this has several advantages:
        /// 1 - JSON.NET will not have to deal with circular references
        /// 2 - Every strong entity is mentioned once in the JSON response (smaller response size)
        /// 3 - It makes it easier for clients to store and track entities in a central workspace
        /// </summary>
        /// <returns>A hash set of strong related entity in the original result entities (excluding the result entities)</returns>
        public static Dictionary<string, IEnumerable<Entity>> FlattenAndTrim(IEnumerable<Entity> resultEntities, CancellationToken cancellation)
        {
            // If the result is empty, nothing to do
            if (resultEntities == null || !resultEntities.Any())
            {
                return new Dictionary<string, IEnumerable<Entity>>();
            }

            var relatedEntities = new HashSet<Entity>();
            var resultHash = resultEntities.ToHashSet();

            // Method for efficiently retrieving the nav and nav collection properties of any entity
            var cacheNavigationProperties = new Dictionary<Type, IEnumerable<PropertyInfo>>();
            IEnumerable<PropertyInfo> NavProps(Entity entity)
            {
                if (!cacheNavigationProperties.TryGetValue(entity.GetType(), out IEnumerable<PropertyInfo> properties))
                {
                    // Return all navigation properties that Entity or list types
                    properties = cacheNavigationProperties[entity.GetType()] =
                        entity.GetType().GetProperties().Where(e =>
                            e.PropertyType.IsEntity() ||  /* nav property */
                            e.PropertyType.IsList()); /* nav collection property */
                }

                return properties;
            }

            // Recursively trims and flattens the entity and all entities reachable from it
            var alreadyFlattenedAndTrimmed = new HashSet<Entity>();
            void FlattenAndTrimInner(Entity entity)
            {
                if (entity == null || alreadyFlattenedAndTrimmed.Contains(entity))
                {
                    return;
                }

                // This ensures Flatten is executed on every entity only once
                alreadyFlattenedAndTrimmed.Add(entity);

                foreach (var navProp in NavProps(entity))
                {
                    if (navProp.PropertyType.IsList())
                    {
                        var collection = navProp.GetValue(entity);
                        if (collection != null)
                        {
                            foreach (var item in collection.Enumerate<Entity>())
                            {
                                FlattenAndTrimInner(item);
                            }
                        }
                    }
                    else if (navProp.GetValue(entity) is Entity relatedEntity) // Checks for null
                    {
                        // If the type is a strong one trim the property and add the entity to relatedEntities
                        if (navProp.PropertyType.IsStrongEntity())
                        {
                            // This property has a strong type, so we set it to null and put its value in the
                            // related entities collection (unless it is part of the main result)

                            // Set the property to null
                            navProp.SetValue(entity, null);
                            if (!resultHash.Contains(relatedEntity))
                            {
                                // Unless it is part of the main result, add it to relatedEntities
                                relatedEntities.Add(relatedEntity);
                            }
                        }

                        // Recursively call flatten on the related entity whether it's strong or weak
                        FlattenAndTrimInner(relatedEntity);
                    }
                }
            }

            // Flatten every entity
            foreach (var entity in resultEntities)
            {
                FlattenAndTrimInner(entity);
                cancellation.ThrowIfCancellationRequested();
            }

            // Return the result
            return relatedEntities
                .GroupBy(e => e.GetType().GetRootType().Name)
                .ToDictionary(g => g.Key, g => g.AsEnumerable()); ;
        }

        //public static Dictionary<string, IEnumerable<Entity>> FlattenAndTrim(IEnumerable<Entity> resultEntities, TypeDescriptor typeDesc, CancellationToken cancellation)
        //{
        //    static FlattenAndTrimInner(Entity entity, TypeDescriptor typeDesc)
        //    {
        //        if (entity == null || entity.EntityMetadata.FlattenedAndTrimmed)
        //        {
        //            return;
        //        }

        //        foreach 
        //    }
        //}
    }
}

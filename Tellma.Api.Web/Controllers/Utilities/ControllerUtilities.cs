using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api;
using Tellma.Api.Base;
using Tellma.Model.Common;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Utilities
{
    public static class ControllerUtilities
    {
        /// <summary>
        /// Calls the provided function and handles the special exceptions by turning them into <see cref="ActionResult"/>s.
        /// Action implementations can then throw these exceptions when there is an error, making the implementation easier.
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
            catch (ValidationException ex)
            {
                return new UnprocessableEntityObjectResult(ToModelState(ex.ModelState));
            }
            catch (ServiceException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error caught in {nameof(InvokeActionImpl)}<{nameof(T)}>: {ex.Message}");
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
            catch (ValidationException ex)
            {
                return new UnprocessableEntityObjectResult(ToModelState(ex.ModelState));
            }
            catch (ServiceException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error caught in {nameof(InvokeActionImpl)}: {ex.Message}");
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
        /// Takes a list of <see cref="Entity"/>'s, and for every entity it inspects the navigation properties, if a navigation property
        /// contains an <see cref="Entity"/> with a strong type, it sets that property to null, and moves the strong entity into a separate
        /// "relatedEntities" hash set, this has several advantages:
        /// 1 - JSON.NET will not have to deal with circular references
        /// 2 - Every strong entity is mentioned once in the JSON response (smaller response size)
        /// 3 - It makes it easier for clients to store and track entities in a central workspace
        /// </summary>
        /// <returns>A dictionary mapping every type name to an <see cref="IEnumerable"/> of related entities of that type (excluding the result entities)</returns>
        public static Dictionary<string, IEnumerable<Entity>> FlattenAndTrim<TEntity>(IEnumerable<TEntity> resultEntities, CancellationToken cancellation)
            where TEntity : Entity
        {
            // If the result is empty, nothing to do
            if (resultEntities == null || !resultEntities.Any())
            {
                return new Dictionary<string, IEnumerable<Entity>>();
            }

            var relatedEntities = new HashSet<Entity>();
            var resultHash = resultEntities.ToHashSet();

            void FlattenAndTrimInner(Entity entity, TypeDescriptor typeDesc)
            {
                if (entity.EntityMetadata.FlattenedAndTrimmed)
                {
                    // This has already been flattened and trimed before
                    return;
                }

                // Mark the entity as flattened and trimmed
                entity.EntityMetadata.FlattenedAndTrimmed = true;

                // Recursively go over the nav properties
                foreach (var prop in typeDesc.NavigationProperties)
                {
                    if (prop.GetValue(entity) is Entity relatedEntity)
                    {
                        prop.SetValue(entity, null);

                        if (!resultHash.Contains(relatedEntity))
                        {
                            // Unless it is part of the main result, add it to relatedEntities
                            relatedEntities.Add(relatedEntity);
                        }

                        FlattenAndTrimInner(relatedEntity, prop.TypeDescriptor);
                    }
                }

                // Recursively go over every entity in the nav collection properties
                foreach (var prop in typeDesc.CollectionProperties)
                {
                    var collectionType = prop.CollectionTypeDescriptor;
                    if (prop.GetValue(entity) is IList collection)
                    {
                        foreach (var obj in collection)
                        {
                            if (obj is Entity relatedEntity)
                            {
                                FlattenAndTrimInner(relatedEntity, collectionType);
                            }
                        }
                    }
                }
            }

            // Flatten every entity in the main list
            var typeDesc = TypeDescriptor.Get<TEntity>();
            foreach (var entity in resultEntities)
            {
                if (entity != null)
                {
                    FlattenAndTrimInner(entity, typeDesc);
                    cancellation.ThrowIfCancellationRequested();
                }
            }

            // Return the result
            return relatedEntities
                .GroupBy(e => e.GetType().Name)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        /// <summary>
        /// Attempts to intelligently guess the content mime type from the file name
        /// </summary>
        public static string ContentType(string fileName)
        {
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }

        /// <summary>
        /// Retrieves the collection name from the Entity type
        /// </summary>
        public static string GetCollectionName(Type entityType)
        {
            return entityType.Name;
        }
    }
}

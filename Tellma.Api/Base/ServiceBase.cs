using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;

namespace Tellma.Api
{
    /// <summary>
    /// The base class for all API services.
    /// </summary>
    public abstract class ServiceBase
    {
        #region Initialization

        public ServiceContext Context { get; private set; }
        public int UserId { get; private set; }

        protected string ExternalUserId => 
            (Context ?? throw new InvalidOperationException($"Accessing {ExternalUserId} before initializing the {GetType().Name}.")).ExternalUserId;
        protected string ExternalEmail =>
            (Context ?? throw new InvalidOperationException($"Accessing {ExternalEmail} before initializing the {GetType().Name}.")).ExternalEmail;
        protected int? TenantId =>
            (Context ?? throw new InvalidOperationException($"Accessing {TenantId} before initializing the {GetType().Name}.")).TenantId;
        protected int? DefinitionId =>
            (Context ?? throw new InvalidOperationException($"Accessing {DefinitionId} before initializing the {GetType().Name}.")).DefinitionId;
        protected DateTime Today =>
            (Context ?? throw new InvalidOperationException($"Accessing {Today} before initializing the {GetType().Name}.")).Today;
        protected CancellationToken Cancellation =>
            (Context ?? throw new InvalidOperationException($"Accessing {Cancellation} before initializing the {GetType().Name}.")).Cancellation;

        public async Task Initialize(ServiceContext ctx)
        {
            Context = ctx ?? throw new ArgumentNullException(nameof(ctx));
            UserId = await OnInitialize();
        }

        /// <summary>
        /// Called every time <see cref="Initialize(ServiceContext)"/> is invoked.
        /// </summary>
        /// <returns>The current user Id.</returns>
        protected abstract Task<int> OnInitialize();

        #endregion

        #region Validation

        /// <summary>
        /// Container the validation errors for the current session.
        /// </summary>
        protected ValidationErrorsDictionary ModelState { get; } = new ValidationErrorsDictionary();

        /// <summary>
        /// Recursively validates a list of entities, and all subsequent entities according to their 
        /// <see cref="TypeMetadata"/>, adds the validation errors if any to the <see cref="ValidationErrorsDictionary"/>, 
        /// appends an optional prefix to the error path.
        /// </summary>
        protected void ValidateList<T>(List<T> entities, TypeMetadata meta, string prefix = "") where T : Entity
        {
            if (entities is null)
            {
                return;
            }

            if (meta is null)
            {
                throw new ArgumentNullException(nameof(meta));
            }

            var validated = new HashSet<Entity>();
            foreach (var (key, errorMsg) in ValidateListInner(entities, meta, validated))
            {
                ModelState.AddModelError(prefix + key, errorMsg);
                if (ModelState.HasReachedMaxErrors)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Recursively validates an entity according to the provided <see cref="TypeMetadata"/>, 
        /// adds the validation errors if any to the <see cref="ValidationErrorsDictionary"/>, 
        /// appends an optional prefix to the error path.
        /// </summary>
        protected void ValidateEntity<T>(T entity, TypeMetadata meta, string prefix = "") where T : Entity
        {
            if (entity is null)
            {
                return;
            }

            if (meta is null)
            {
                throw new ArgumentNullException(nameof(meta));
            }

            var validated = new HashSet<Entity>();
            foreach (var (key, errorMsg) in ValidateEntityInner(entity, meta, validated))
            {
                ModelState.AddModelError(prefix + key, errorMsg);
                if (ModelState.HasReachedMaxErrors)
                {
                    return;
                }
            }
        }

        private static IEnumerable<(string key, string error)> ValidateListInner(IList entities, TypeMetadata meta, HashSet<Entity> validated)
        {
            for (int index = 0; index < entities.Count; index++)
            {
                var atIndex = entities[index];
                if (atIndex is null)
                {
                    continue;
                }
                else if (atIndex is Entity entity)
                {
                    if (!validated.Contains(entity))
                    {
                        validated.Add(entity);
                        foreach (var (key, error) in ValidateEntityInner(entity, meta, validated))
                        {
                            yield return ($"[{index}].{key}", error);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Bug: Only entities can be validated with {nameof(ValidateList)}. {atIndex.GetType().Name} does not derive from {nameof(Entity)}.");
                }
            }
        }

        private static IEnumerable<(string key, string error)> ValidateEntityInner<T>(T entity, TypeMetadata meta, HashSet<Entity> validated) where T : Entity
        {
            foreach (var p in meta.SimpleProperties)
            {
                var value = p.Descriptor.GetValue(entity);
                var results = p.Validate(entity, value);

                foreach (var result in results)
                {
                    yield return (p.Descriptor.Name, result.ErrorMessage);
                }
            }

            foreach (var p in meta.NavigationProperties)
            {
                var valueMeta = p.TargetTypeMetadata;
                if (p.Descriptor.GetValue(entity) is Entity value && !validated.Contains(value))
                {
                    validated.Add(value);
                    foreach (var (key, msg) in ValidateEntityInner(entity, valueMeta, validated))
                    {
                        yield return ($"{p.Descriptor.Name}.{key}", msg);
                    }
                }
            }

            foreach (var p in meta.CollectionProperties)
            {
                var valueMeta = p.CollectionTargetTypeMetadata;
                var value = p.Descriptor.GetValue(entity);
                if (value is IList list)
                {
                    var listMeta = p.CollectionTargetTypeMetadata;
                    foreach (var (key, msg) in ValidateListInner(list, listMeta, validated))
                    {
                        yield return ($"{p.Descriptor.Name}{key}", msg);
                    }
                }
                else if (value is null)
                {
                    // Nothing to do
                }
                else
                {
                    throw new InvalidOperationException($"Bug: {nameof(CollectionPropertyDescriptor)}.{nameof(CollectionPropertyDescriptor.GetValue)} returned a non-list.");
                }
            }
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Metadata;
using Tellma.Model.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// The base class for all API services.
    /// </summary>
    public abstract class ServiceBase
    {
        public ServiceBase(IServiceContextAccessor contextAccessor)
        {
            ExternalUserId = contextAccessor.ExternalUserId;
        }

        #region Behavior

        /// <summary>
        /// When implemented, returns <see cref="IServiceBehavior"/> that is invoked every 
        /// time <see cref="Initialize()"/> is invoked.
        /// </summary>
        protected abstract IServiceBehavior Behavior { get; }

        #endregion

        #region Initialization

        private int? _userId;

        /// <summary>
        /// The Id of the currently authenticated user in the currently accessed database.
        /// <summary/>
        public int UserId => _userId ?? throw new InvalidOperationException($"Accessing {UserId} before initializing the {GetType().Name}.");

        /// <summary>
        /// The external user Id from the identity provider.
        /// <summary/>
        protected string ExternalUserId { get; private set; }

        /// <summary>
        /// The external user email from the identity provider.
        /// <summary/>
        protected string ExternalEmail { get; private set; }

        /// <summary>
        /// An optional tenant Id for services that access per-tenant resources.
        /// <summary/>
        protected int? TenantId { get; private set; }

        /// <summary>
        /// An optional definition Id for services that are accessing definitioned resources.
        /// <summary/>
        protected int? DefinitionId { get; private set; }

        /// <summary>
        /// An optional date value to indicate the current date at the client's time zone.
        /// <summary/>
        protected DateTime Today { get; private set; }

        /// <summary>
        /// The cancellation instruction for the service request.
        /// <summary/>
        protected CancellationToken Cancellation { get; private set; }

        /// <summary>
        /// Initializes the service with the contextual information in <paramref name="ctx"/>, this 
        /// method must be invoked before executing any request that relies on this contextual information.
        /// The method also runs any custom initialization logic that is supplied by the implementing service.
        /// </summary>
        public async Task Initialize()
        {
            if (Behavior is null)
            {
                throw new InvalidOperationException($"Bug: {GetType().Name}.Initializer returned null.");
            }

            _userId = await Behavior.OnInitialize();
        }

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

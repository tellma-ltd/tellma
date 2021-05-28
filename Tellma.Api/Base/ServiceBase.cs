using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Model.Common;

namespace Tellma.Controllers
{
    /// <summary>
    /// The base class for all API services.
    /// </summary>
    public abstract class ServiceBase
    {
        #region Initialization

        /// <summary>
        /// When implemented, returns <see cref="IServiceInitializer"/> that is invoked every 
        /// time <see cref="Initialize(ServiceContext)"/> is invoked.
        /// </summary>
        protected abstract IServiceInitializer Initializer { get; }

        private ServiceContext _ctx;
        private int? _userId;

        /// <summary>
        /// The Id of the currently authenticated user in the currently accessed database.
        /// <para/>
        /// Note: This property is only accessible after <see cref="Initialize(ServiceContext)"/> has been invoked.
        /// Accessing the property before that will throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public int UserId => _userId ?? throw new InvalidOperationException($"Accessing {UserId} before initializing the {GetType().Name}.");

        /// <summary>
        /// The external user Id from the identity provider.
        /// <para/>
        /// Note: This property is only accessible after <see cref="Initialize(ServiceContext)"/> has been invoked.
        /// Accessing the property before that will throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected string ExternalUserId =>
            (_ctx ?? throw new InvalidOperationException($"Accessing {ExternalUserId} before initializing the {GetType().Name}.")).ExternalUserId;

        /// <summary>
        /// The external user email from the identity provider.
        /// <para/>
        /// Note: This property is only accessible after <see cref="Initialize(ServiceContext)"/> has been invoked.
        /// Accessing the property before that will throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected string ExternalEmail =>
            (_ctx ?? throw new InvalidOperationException($"Accessing {ExternalEmail} before initializing the {GetType().Name}.")).ExternalEmail;

        /// <summary>
        /// An optional tenant Id for services that access per-tenant resources.
        /// <para/>
        /// Note: This property is only accessible after <see cref="Initialize(ServiceContext)"/> has been invoked.
        /// Accessing the property before that will throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected int? TenantId =>
            (_ctx ?? throw new InvalidOperationException($"Accessing {TenantId} before initializing the {GetType().Name}.")).TenantId;

        /// <summary>
        /// An optional definition Id for services that are accessing definitioned resources.
        /// <para/>
        /// Note: This property is only accessible after <see cref="Initialize(ServiceContext)"/> has been invoked.
        /// Accessing the property before that will throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected int? DefinitionId =>
            (_ctx ?? throw new InvalidOperationException($"Accessing {DefinitionId} before initializing the {GetType().Name}.")).DefinitionId;

        /// <summary>
        /// An optional date value to indicate the current date at the client's time zone.
        /// <para/>
        /// Note: This property is only accessible after <see cref="Initialize(ServiceContext)"/> has been invoked.
        /// Accessing the property before that will throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected DateTime Today =>
            (_ctx ?? throw new InvalidOperationException($"Accessing {Today} before initializing the {GetType().Name}.")).Today;

        /// <summary>
        /// The cancellation instruction for the service request.
        /// <para/>
        /// Note: This property is only accessible after <see cref="Initialize(ServiceContext)"/> has been invoked.
        /// Accessing the property before that will throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected CancellationToken Cancellation =>
            (_ctx ?? throw new InvalidOperationException($"Accessing {Cancellation} before initializing the {GetType().Name}.")).Cancellation;

        /// <summary>
        /// Initializes the service with the contextual information in <paramref name="ctx"/>, this 
        /// method must be invoked before executing any request that relies on this contextual information.
        /// The method also runs any custom initialization logic that is supplied by the implementing service.
        /// </summary>
        /// <param name="ctx">The contextual </param>
        /// <returns></returns>
        public async Task Initialize(ServiceContext ctx)
        {
            if (Initializer is null)
            {
                throw new InvalidOperationException($"Bug: {GetType().Name}.Initializer returned null.");
            }

            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _userId = await Initializer.OnInitialize(ctx);
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

﻿using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tellma.Api.Metadata;
using Tellma.Model.Common;
using Tellma.Repository.Common;

namespace Tellma.Api.Base
{
    /// <summary>
    /// The base class for all API services.
    /// </summary>
    public abstract class ServiceBase : IServiceBase
    {
        #region Lifecycle

        private IServiceContextAccessor _contextAccessor;
        private IServiceContextAccessor _contextAccessorWhenInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase"/> class.
        /// </summary>
        /// <param name="contextAccessor"></param>
        public ServiceBase(IServiceContextAccessor contextAccessor)
        {
            // Default
            SetContext(contextAccessor);
        }

        /// <summary>
        /// Overrides the default <see cref="IServiceContextAccessor"/> with a custom one.
        /// </summary>
        public void SetContext(IServiceContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        /// <summary>
        /// Initializes the service with the contextual information in <paramref name="ctx"/>, this 
        /// method must be invoked before executing any request that relies on this contextual information.
        /// The method also runs any custom initialization logic that is supplied by the implementing service.
        /// Any subsequent calls to this method will have no effect.
        /// </summary>
        protected async Task Initialize(CancellationToken cancellation = default)
        {
            // Don't run initialize more than once for the same context accessor
            if (_contextAccessorWhenInitialized != _contextAccessor)
            {
                _contextAccessorWhenInitialized = _contextAccessor;

                if (Behavior is null)
                {
                    throw new InvalidOperationException($"Bug: {GetType().Name}.{nameof(Behavior)} returned null.");
                }

                _userId = await Behavior.OnInitialize(_contextAccessor, cancellation);
            }
        }

        /// <summary>
        /// Backing field for <see cref="UserId"/>.
        /// </summary>
        private int? _userId;

        /// <summary>
        /// The Id of the currently authenticated user in the currently accessed database.
        /// <summary/>
        public int UserId => _userId ?? throw new InvalidOperationException($"Accessing {UserId} before initializing the {GetType().Name}.");

        /// <summary>
        /// The external user Id from the identity provider.
        /// <summary/>
        protected string ExternalUserId => _contextAccessor.ExternalUserId;

        /// <summary>
        /// The external user email from the identity provider.
        /// <summary/>
        protected string ExternalEmail => _contextAccessor.ExternalEmail;

        /// <summary>
        /// Whether or not the currently authenticated user is a service account or a human.
        /// <summary/>
        protected bool IsServiceAccount => _contextAccessor.IsServiceAccount;

        /// <summary>
        /// An optional tenant Id for services that access per-tenant resources.
        /// <summary/>
        protected int? TenantId => _contextAccessor.TenantId;

        /// <summary>
        /// An optional date value to indicate the current date at the client's time zone.
        /// <summary/>
        protected DateTime Today => _contextAccessor.Today;

        /// <summary>
        /// An optional string value to indicate the calendar used at the client to display dates.
        /// <summary/>
        protected string Calendar => _contextAccessor.Calendar;

        #endregion

        #region Behavior

        /// <summary>
        /// When implemented, returns <see cref="IServiceBehavior"/> that is invoked every 
        /// time <see cref="Initialize()"/> is invoked.
        /// </summary>
        protected abstract IServiceBehavior Behavior { get; }

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
                ModelState.AddError(prefix + key, errorMsg);
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
                ModelState.AddError(prefix + key, errorMsg);
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

        #region Validation

        /// <summary>
        /// The method localizes every error in the collection and adds it to <see cref="ModelState"/>.
        /// </summary>
        public void AddLocalizedErrors(IEnumerable<ValidationError> errors, IStringLocalizer localizer)
        {
            foreach (var error in errors)
            {
                object[] formattedArgs = FormatArguments(error, localizer);

                string key = error.Key;
                string errorMessage = localizer[error.ErrorName, formattedArgs];

                ModelState.AddError(key: key, errorMessage: errorMessage);
            }
        }

        /// <summary>
        /// SQL validation may return error message names (for localization) as well as some arguments 
        /// this method parses those arguments into objects based on their prefix for example date:2019-01-13
        /// will be parsed to datetime object suitable for formatting in C# into the error message.
        /// </summary>
        public static object[] FormatArguments(ValidationError error, IStringLocalizer localizer)
        {
            static object Parse(string str, IStringLocalizer localizer)
            {
                // Null returns null
                if (string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }

                // Anything with this prefix is translated
                const string translateKey = "localize:";
                if (str.StartsWith(translateKey))
                {
                    str = str.Remove(0, translateKey.Length);
                    return localizer[str];
                }

                return str;
            }

            object[] formatArguments = {
                    Parse(error.Argument1, localizer),
                    Parse(error.Argument2, localizer),
                    Parse(error.Argument3, localizer),
                    Parse(error.Argument4, localizer),
                    Parse(error.Argument5, localizer)
                };

            return formatArguments;
        }

        #endregion

        #endregion
    }

    public interface IServiceBase
    {
        void SetContext(IServiceContextAccessor contextAccessor);
    }
}

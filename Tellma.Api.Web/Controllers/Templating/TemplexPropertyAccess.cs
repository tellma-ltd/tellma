using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;
using Tellma.Data.Queries;
using Tellma.Model.Application;

namespace Tellma.Controllers.Templating
{
    /// <summary>
    /// Represents accessing the property of an <see cref="Entity"/> object. E.g. doc.Memo.
    /// Evaluates to the value of that property
    /// </summary>
    public class TemplexPropertyAccess : TemplexBase
    {
        /// <summary>
        /// Cache, to avoid computing it twice
        /// </summary>
        private IAsyncEnumerable<Path> _entityPaths;

        /// <summary>
        /// The expression that evaluates to a model <see cref="Entity"/> whose property we're accessing
        /// </summary>
        public TemplexBase EntityCandidate { get; set; } // Must evaluate to a model entity

        /// <summary>
        /// The name of the property we're accessing on the <see cref="Entity"/>
        /// </summary>
        public string PropertyName { get; set; }

        public override async IAsyncEnumerable<Path> ComputeSelect(EvaluationContext ctx)
        {
            await foreach (var select in EntityCandidate.ComputeSelect(ctx))
            {
                yield return select;
            }

            _entityPaths ??= EntityCandidate.ComputePaths(ctx);
            await foreach (var path in _entityPaths)
            {
                yield return path.Append(PropertyName);
            }
        }

        public override async IAsyncEnumerable<Path> ComputePaths(EvaluationContext ctx)
        {
            _entityPaths ??= EntityCandidate.ComputePaths(ctx);
            await foreach (var path in _entityPaths)
            {
                yield return path.Append(PropertyName); // <-- Best guess, since we don't know if the property is simple or nav
            }
        }

        public override async Task<object> Evaluate(EvaluationContext ctx)
        {
            var entityCandidate = await EntityCandidate.Evaluate(ctx);
            if (entityCandidate == null)
            {
                // Template property access implements null propagation out of the box
                return null;
            }

            if (entityCandidate is Entity entity)
            {
                if (entity is EntityWithKey entityWithKey && PropertyName == "Id") 
                {
                    return entityWithKey.GetId();
                }

                var entityType = entity.GetType();
                var propInfo = entityType.GetProperty(PropertyName);
                if (propInfo == null || propInfo.GetCustomAttribute<NotMappedAttribute>() != null)
                {
                    throw new TemplateException($"Property '{PropertyName}' does not exist on type {entityType.Name}");
                }
                else if (!entity.EntityMetadata.TryGetValue(PropertyName, out FieldMetadata meta))
                {
                    throw new TemplateException($"Property '{PropertyName}' on type {entityType.Name} was not loaded correctly"); // Developer mistake
                }
                else if (meta == FieldMetadata.Restricted)
                {
                    throw new TemplateException($"Your account is not granted access to property '{PropertyName}' on type '{entityType.Name}'");
                }
                else if (meta == FieldMetadata.Loaded)
                {
                    return propInfo.GetValue(entity);
                }
                else
                {
                    // For future proofing
                    throw new TemplateException($"Unknown FieldMetadata value '{meta}'");
                }
            }
            else
            {
                throw new TemplateException($"Property access '.{PropertyName}' is only valid on model entities");
            }
        }

        /// <summary>
        /// Creates a new <see cref="TemplexPropertyAccess"/>
        /// </summary>
        public static TemplexPropertyAccess Make(TemplexBase entityCandidate, string propName)
        {
            return new TemplexPropertyAccess
            {
                EntityCandidate = entityCandidate,
                PropertyName = propName
            };
        }

        public override string ToString()
        {
            return $"{EntityCandidate}.{PropertyName}";
        }
    }
}

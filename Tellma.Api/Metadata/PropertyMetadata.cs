using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Api
{
    /// <summary>
    /// Wrapper around <see cref="PropertyDescriptor"/>. It provides additional metadata that may be influenced
    /// by the entity definition. For example the definition may hide certain properties or change their labels.
    /// </summary>
    public class PropertyMetadata
    {
        private readonly Func<string> _display;
        private readonly Func<Entity, object, IEnumerable<ValidationResult>> _validate;
        private readonly Func<object, string> _format;
        private readonly Func<string, object> _parse;

        /// <summary>
        /// The <see cref="PropertyDescriptor"/> wrapped by this <see cref="PropertyMetadata"/>.
        /// </summary>
        public PropertyDescriptor Descriptor { get; }

        /// <summary>
        /// The display label of the function, computed based on the current thread culture.
        /// </summary>
        public string Display() => _display();

        /// <summary>
        /// Performs basic validation from property attributes (such as <see cref="RequiredAttribute"/>) or from definition.
        /// </summary>
        /// <param name="entity">The entity being validated.</param>
        /// <param name="value">The property value being validated.</param>
        /// <returns>
        /// A collection of all validation errors with localized error messages
        /// or an empty <see cref="IEnumerable{T}"/> if the value is valid.
        /// </returns>
        public IEnumerable<ValidationResult> Validate(Entity entity, object value) => _validate(entity, value);

        /// <summary>
        /// Formats the property value into its string representation.
        /// This method is the opposite of <see cref="Parse(string)"/>.
        /// </summary>
        public virtual string Format(object value) => _format != null ? _format(value) : throw new InvalidOperationException($"Bug: {nameof(Format)} was invoked on {Display()} property without a backing field");

        /// <summary>
        /// Parses a string representation (Coming from an imported CSV or Excel file) into a value.
        /// This method is the opposite of <see cref="Format(object)"/>.
        /// </summary>
        /// <exception cref="ParseException"></exception>
        public object Parse(string stringValue) => _parse != null ? _parse(stringValue) : throw new InvalidOperationException($"Bug: {nameof(Parse)} was invoked on {Display()} property without a backing field");

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMetadata"/> class.
        /// </summary>
        public PropertyMetadata(
            PropertyDescriptor desc,
            Func<string> display,
            Func<Entity, object, IEnumerable<ValidationResult>> validate,
            Func<object, string> format,
            Func<string, object> parse
            )
        {
            Descriptor = desc;

            // Specific to metadata
            _display = display ?? throw new ArgumentNullException(nameof(display));
            _validate = validate ?? throw new ArgumentNullException(nameof(validate));
            _format = format;
            _parse = parse;
        }
    }
}

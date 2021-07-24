using System.Collections.Generic;
using System.Linq;

namespace Tellma.Api.Base
{
    /// <summary>
    /// Stores the errors during an API service request.
    /// </summary>
    public class ValidationErrorsDictionary
    {
        public const int DefaultMaxAllowedErrors = 200;

        private readonly Dictionary<string, HashSet<string>> _dic = new();

        public IEnumerable<(string Key, IEnumerable<string> Errors)> AllErrors => _dic.Select(e => (e.Key, (IEnumerable<string>)e.Value));

        public bool ContainsKey(string key)
        {
            return _dic.ContainsKey(key);
        }

        /// <summary>
        /// Adds the given error message to the collection under the given key and increments
        /// the <see cref="ErrorCount"/> unless the key and errorMessage already exist.
        /// </summary>
        internal void AddModelError(string key, string errorMessage)
        {
            if (!HasReachedMaxErrors)
            {
                if (!_dic.TryGetValue(key, out HashSet<string> set))
                {
                    set = new HashSet<string>();
                    _dic.Add(key, set);
                }

                if (set.Add(errorMessage))
                {
                    ErrorCount++;
                }
            }
        }

        /// <summary>
        /// Clears all errors if any.
        /// </summary>
        internal void ClearErrors()
        {
            _dic.Clear();
        }

        /// <summary>
        /// The maximum number of errors that this <see cref="ValidationErrorsDictionary"/> 
        /// will accept.
        /// </summary>
        public static int MaxAllowedErrors => DefaultMaxAllowedErrors;

        /// <summary>
        /// How many errors can still be added to this <see cref="ValidationErrorsDictionary"/>.
        /// </summary>
        public int RemainingErrors => MaxAllowedErrors - ErrorCount;

        /// <summary>
        /// Total number of unique key and error message pairs added.
        /// </summary>
        public int ErrorCount { get; private set; } = 0;

        /// <summary>
        /// True if and only if no errors were added since the creation of this dictionary.
        /// </summary>
        internal bool IsValid => ErrorCount == 0;

        /// <summary>
        /// Syntactic sugar for <see cref="ErrorCount"/> >= <see cref="MaxAllowedErrors"/>.
        /// </summary>
        internal bool HasReachedMaxErrors => ErrorCount >= MaxAllowedErrors;

        /// <summary>
        /// If <see cref="IsValid"/> is false, throw an <see cref="ValidationException"/>
        /// containing the validation errors within.
        /// </summary>
        /// <exception cref="ValidationException"></exception>
        internal void ThrowIfInvalid()
        {
            if (!IsValid)
            {
                throw new ValidationException(this);
            }
        }
    }
}

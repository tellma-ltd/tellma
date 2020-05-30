using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace Tellma.Controllers.Utilities
{
    public class ValidationErrorsDictionary
    {
        private readonly Dictionary<string, List<string>> _dic = new Dictionary<string, List<string>>();

        public IEnumerable<(string Key, IEnumerable<string> Errors)> AllErrors => _dic.Select(e => (e.Key, (IEnumerable<string>)e.Value));

        public bool ContainsKey(string key)
        {
            return _dic.ContainsKey(key);
        }

        public void AddModelError(string key, string errorMessage)
        {
            if (!HasReachedMaxErrors)
            {
                if (!_dic.TryGetValue(key, out List<string> list))
                {
                    list = new List<string>();
                    _dic.Add(key, list);
                }

                list.Add(errorMessage);
                ErrorCount++;
            }
        }

        public int MaxAllowedErrors => ModelStateDictionary.DefaultMaxAllowedErrors;

        public int ErrorCount { get; private set; } = 0;

        public bool IsValid => ErrorCount == 0;

        public bool HasReachedMaxErrors => ErrorCount >= MaxAllowedErrors;

        public void ThrowIfInvalid()
        {
            if (!IsValid)
            {
                throw new UnprocessableEntityException(this);
            }
        }
    }
}

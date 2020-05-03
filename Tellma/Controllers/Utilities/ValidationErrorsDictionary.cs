using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            }
        }

        public int MaxAllowedErrors => ModelStateDictionary.DefaultMaxAllowedErrors;

        public int ErrorCount => _dic.Keys.Count;

        public bool IsValid => ErrorCount == 0;

        public bool HasReachedMaxErrors => ErrorCount >= MaxAllowedErrors;
    }
}

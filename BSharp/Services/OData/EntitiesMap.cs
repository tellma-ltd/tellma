using BSharp.Controllers.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BSharp.Services.OData
{
    public class EntitiesMap : Dictionary<string, IEnumerable<DtoBase>> { }


    public class IndexedEntities : Dictionary<Type, IndexedEntitiesOfType> { }
    //{
    //    private readonly Dictionary<Type, IndexedEntitiesOfType> _store = new Dictionary<Type, IndexedEntitiesOfType>();

    //    public DtoBase this[Type t, object id]
    //    {
    //        get
    //        {
    //            _store.TryGetValue(t, out IndexedEntitiesOfType ofType);
    //            if (ofType != null)
    //            {
    //                return ofType[id];
    //            }
    //        }

    //        set
    //        {

    //        }
    //    }
    //}

    public class IndexedEntitiesOfType : Dictionary<object, DtoKeyBase> { }
    //{
    //    private readonly Dictionary<int, DtoBase> _intKeyEntities = new Dictionary<int, DtoBase>();
    //    private readonly Dictionary<string, DtoBase> _stringKeyEntities = new Dictionary<string, DtoBase>();

    //    public IEnumerable<DtoBase> Values { get => _intKeyEntities.Values.Concat(_stringKeyEntities.Values); }

    //    public DtoBase this[object id]
    //    {
    //        get
    //        {
    //            DtoBase result;
    //            if (id is int intId)
    //            {
    //                _intKeyEntities.TryGetValue(intId, out result);
    //            }
    //            else if (id is string stringId)
    //            {
    //                _stringKeyEntities.TryGetValue(stringId, out result);
    //            }
    //            else
    //            {
    //                // Programmer mistake
    //                throw new InvalidOperationException("Only Id types int and string are supported for DTOs");
    //            }

    //            return result;
    //        }

    //        set
    //        {
    //            if (id is int intId)
    //            {
    //                _intKeyEntities[intId] = value;
    //            }
    //            else if (id is string stringId)
    //            {
    //                _stringKeyEntities[stringId] = value;
    //            }
    //            else
    //            {
    //                // Programmer mistake
    //                throw new InvalidOperationException("Only Id types int and string are supported for DTOs");
    //            }
    //        }
    //    }

    //    public void Remove(object id)
    //    {
    //        if (id is int intId)
    //        {
    //            _intKeyEntities.Remove(intId);
    //        }
    //        else if (id is string stringId)
    //        {
    //            _stringKeyEntities.Remove(stringId);
    //        }
    //        else
    //        {
    //            // Programmer mistake
    //            throw new InvalidOperationException("Only Id types int and string are supported for DTOs");
    //        }
    //    }
    // }
}

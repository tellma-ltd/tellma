using BSharp.Controllers.Misc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BSharp.Controllers.DTO
{
    // Rule 1: No DTO class can contain a property Id unless it inherits from DtoKeyBase
    // Rule 2: Every class that inherits from DtoKeyBase must contain a property "Id"

    public abstract class DtoKeyBase : DtoBase
    {
        /// <summary>
        /// All inheriting classes will have a strongly typed Id that is usually an int or an object,
        /// this method returns either as an object, it is useful for logic that performs reflection
        /// </summary>
        /// <returns></returns>
        public abstract object GetId();

        public abstract void SetId(object id);
    }

    /// <summary>
    /// Base class of all DTOs that have an Id property
    /// </summary>
    /// <typeparam name="TKey">The type of the Id property</typeparam>
    public abstract class DtoKeyBase<TKey> : DtoKeyBase
    {
        /// <summary>
        /// This is an integer for entities that have a simple integer key in the SQL database,
        /// and a string for anything else (The string can encode composite keys for example) 
        /// it is important to have a single Id property for tracking HTTP resources as it simplifies
        /// so much shared logic for tracking resources and caching them
        /// </summary>
        [IgnoreInMetadata]
        public TKey Id { get; set; }

        // The below method is used by implementations that benefit from a generic object Id, such as Object Loader

        private object _id;
        public override object GetId()
        {
            if(_id == null) // Optimization: if statement is faster than boxing-unboxing
            {
                _id = Id;
            }

            return _id;
        }

        public override void SetId(object id)
        {
            Id = (TKey)id;
            _id = id;
        }
    }
}

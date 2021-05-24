using System.ComponentModel.DataAnnotations;

namespace Tellma.Model.Common
{
    /// <summary>
    /// Base class of all entities
    /// <para/>
    /// Rule 1: No <see cref="Entity"/> class can contain a property Id unless it inherits from <see cref="EntityWithKey"/>.<br/>
    /// Rule 2: Every class that inherits from <see cref="EntityWithKey"/> must contain a property "Id".
    /// </summary>
    public abstract class EntityWithKey : Entity
    {
        /// <summary>
        /// All inheriting classes will have a strongly typed Id property that is either an int or a string,
        /// this method returns either one as an object, it is useful for logic that performs reflection.
        /// </summary>
        public abstract object GetId();

        /// <summary>
        /// All inheriting classes will have a strongly typed Id property that is either an int or a string,
        /// this method allows setting either one as an object, which is faster than doing it via reflection.
        /// </summary>
        public abstract void SetId(object id);
    }

    /// <summary>
    /// Base class of all entities that have an Id property
    /// </summary>
    /// <typeparam name="TKey">The type of the Id property</typeparam>
    public abstract class EntityWithKey<TKey> : EntityWithKey
    {
        /// <summary>
        /// This is an integer for entities that have a simple integer Id in the SQL database,
        /// and a string for anything else (The string can encode composite keys for example) 
        /// it is important to have a single Id property for tracking resources as it simplifies
        /// the shared logic for tracking and caching them
        /// </summary>
        [Display(Name = "Id")]
        [Required]
        public TKey Id { get; set; }

        // The below method is used by implementations that benefit from a generic object Id, such as Object Loader

        private object _id;
        public override object GetId()
        {
            // Optimization: if statement is faster than boxing TKey Id into an object every single time
            if (_id == null)
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

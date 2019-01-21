using BSharp.Controllers.Misc;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// The base class for all DTOs of HTTP resources that can be modified
    /// </summary>
    public abstract class DtoForSaveKeyBase<TKey> : DtoKeyBase<TKey>
    {
        /// <summary>
        /// Either 'Inserted' or 'Updated' or 'Deleted'
        /// </summary>
        [ChoiceList(choices: new object[] { "Inserted", "Updated", "Deleted" })]
        public string EntityState { get; set; }
    }
}

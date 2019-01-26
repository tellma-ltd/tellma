using BSharp.Controllers.Misc;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BSharp.Controllers.DTO
{
    /// <summary>
    /// The base class for all DTOs of HTTP resources that can be modified
    /// </summary>
    public abstract class DtoForSaveKeyBase<TKey> : DtoKeyBase<TKey>, IValidatableObject
    {
        /// <summary>
        /// Either 'Inserted' or 'Updated' or 'Deleted'
        /// </summary>
        [ChoiceList(choices: new object[] { EntityStates.Inserted, EntityStates.Updated, EntityStates.Deleted })]
        public string EntityState { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Here we validate that Id makes sense together with EntityState
            if (typeof(TKey) == typeof(int?))
            {
                var _localizer = (IStringLocalizer<DtoForSaveKeyBase<TKey>>)validationContext.GetService(typeof(IStringLocalizer<DtoForSaveKeyBase<TKey>>));
                if (Id != null && EntityState == EntityStates.Inserted)
                {
                    // This error indicates a bug
                    yield return new ValidationResult(_localizer["Error_CannotInsertWhileSpecifyId"], new string[] { nameof(Id) });
                }

                if (Id == null && EntityState == EntityStates.Updated)
                {
                    // This error indicates a bug
                    yield return new ValidationResult(_localizer["Error_CannotUpdateWithoutId"], new string[] { nameof(Id) });
                }

                if (Id == null && EntityState == EntityStates.Deleted)
                {
                    // This error indicates a bug
                    yield return new ValidationResult(_localizer["Error_CannotDeleteWithoutId"], new string[] { nameof(Id) });
                }
            }
        }
    }
}

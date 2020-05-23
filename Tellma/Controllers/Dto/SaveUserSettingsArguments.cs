using System.ComponentModel.DataAnnotations;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Dto
{
    public class SaveUserSettingsArguments
    {
        [StringLength(255, ErrorMessage = nameof(StringLengthAttribute))]
        [Required(ErrorMessage = Constants.Error_TheField0IsRequired)]
        public string Key { get; set; }

        [StringLength(2048, ErrorMessage = nameof(StringLengthAttribute))]
        public string Value { get; set; }
    }
}

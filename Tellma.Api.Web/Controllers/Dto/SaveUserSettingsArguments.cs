using System.ComponentModel.DataAnnotations;
using Tellma.Services.Utilities;

namespace Tellma.Controllers.Dto
{
    public class SaveUserSettingsArguments
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

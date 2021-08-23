using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Repository.Application
{
    public class GenerateArgument : Entity
    {
        [StringLength(50)]
        public string Key { get; set; }

        [StringLength(255)]
        public string Value { get; set; }
    }
}

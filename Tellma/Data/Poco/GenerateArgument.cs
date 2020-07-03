using System.ComponentModel.DataAnnotations;
using Tellma.Entities;

namespace Tellma.Data
{
    public class GenerateArgument : Entity
    {
        [StringLength(50)]
        public string Key { get; set; }
        
        [StringLength(255)]
        public string Value { get; set; }
    }
}

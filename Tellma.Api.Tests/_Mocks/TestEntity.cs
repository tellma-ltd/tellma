using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Api.Tests
{
    public class TestEntity : EntityWithKey<int?>
    {
        [Display(Name = "Test Name")]
        public string Name { get; set; }

        [Display(Name = "Test Age")]
        public int? Age { get; set; }

        [Display(Name = "Test Hidden")]
        public string Hidden { get; set; }
    }
}

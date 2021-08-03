using System.ComponentModel.DataAnnotations;
using Tellma.Model.Common;

namespace Tellma.Repository.Common.Tests
{
    internal class TestEntity : EntityWithKey<int>
    {
        [Required]
        public int Foo { get; set; }
        public string Bar { get; set; }
    }
}

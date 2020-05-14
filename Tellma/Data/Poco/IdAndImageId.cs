using Tellma.Entities;

namespace Tellma.Data
{
    public class IndexedImageId : Entity
    {
        public int Index { get; set; }
        public string ImageId { get; set; }
    }
}

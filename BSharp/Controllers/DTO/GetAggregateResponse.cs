using BSharp.EntityModel;

namespace BSharp.Controllers.Dto
{
    public class GetAggregateResponse : EntitiesResponse<Entity>
    {
        public int Top { get; set; }
    }
}

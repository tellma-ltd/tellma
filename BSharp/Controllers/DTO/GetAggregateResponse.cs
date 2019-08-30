using BSharp.Entities;

namespace BSharp.Controllers.Dto
{
    public class GetAggregateResponse : EntitiesResponse<Entity>
    {
        public int Top { get; set; }
    }
}

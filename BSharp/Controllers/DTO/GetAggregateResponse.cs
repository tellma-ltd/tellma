using BSharp.Data.Queries;

namespace BSharp.Controllers.Dto
{
    public class GetAggregateResponse : EntitiesResponse<DynamicEntity>
    {
        public int Top { get; set; }
    }
}

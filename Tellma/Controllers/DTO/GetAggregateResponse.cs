using Tellma.Data.Queries;

namespace Tellma.Controllers.Dto
{
    public class GetAggregateResponse : EntitiesResponse<DynamicEntity>
    {
        public int Top { get; set; }
    }
}

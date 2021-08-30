namespace Tellma.Api.Dto
{
    public class GetEntityResponse<TEntity>
    {
        public TEntity Result { get; set; }

        public RelatedEntities RelatedEntities { get; set; }
    }
}

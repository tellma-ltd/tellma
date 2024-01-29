namespace Tellma.Model.Common
{
    public interface IEntityWithCustomFields<T> where T : CustomFieldsBase
    {
        public string CustomFieldsJson { get; set; }
        public T CustomFields { get; set; }
    }
}

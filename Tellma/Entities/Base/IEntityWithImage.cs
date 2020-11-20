namespace Tellma.Entities
{
    public interface IEntityWithImage
    {
        int Id { get; set; }

        byte[] Image { get; set; }
    }
}

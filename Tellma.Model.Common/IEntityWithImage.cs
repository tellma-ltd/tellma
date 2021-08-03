namespace Tellma.Model.Common
{
    public interface IEntityWithImage
    {
        int Id { get; set; }

        byte[] Image { get; set; }
    }
}

namespace BSharp.Entities
{
    public interface IEntityWithImageForSave
    {
        int Id { get; set; }

        byte[] Image { get; set; }
    }

    public interface IEntityWithImage : IEntityWithImageForSave
    {
        string ImageId { get; set; }
    }
}

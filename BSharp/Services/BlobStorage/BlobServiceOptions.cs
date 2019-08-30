namespace BSharp.Services.BlobStorage
{
    public class BlobServiceOptions
    {
        public AzureBlobStorageOptions AzureBlobStorage { get; set; } = new AzureBlobStorageOptions();
    }
}

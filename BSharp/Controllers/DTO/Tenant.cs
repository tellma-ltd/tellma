namespace BSharp.Controllers.DTO
{

    public class TenantForSave : DtoForSaveKeyBase<int?>
    {

    }

    public class Tenant : TenantForSave
    {

    }

    public class TenantForClient : DtoBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }
        public string ImageId { get; set; }
    }
}

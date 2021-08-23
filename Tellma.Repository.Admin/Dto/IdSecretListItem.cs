using Tellma.Model.Common;

namespace Tellma.Repository.Admin
{
    public class IdSecretListItem : EntityWithKey<int>
    {
        public string ClientSecret { get; set; }
    }
}

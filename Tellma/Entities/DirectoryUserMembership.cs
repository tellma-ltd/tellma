namespace Tellma.Entities
{
    public class DirectoryUserMembershipForSave : EntityWithKey<int>
    {
        public int? UserId { get; set; }

        public int? DatabaseId { get; set; }
    }

    public class DirectoryUserMembership : DirectoryUserMembershipForSave
    {
        public DirectoryUser User { get; set; }

        public SqlDatabase Database { get; set; }
    }
}

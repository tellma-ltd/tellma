namespace Tellma.Entities
{
    public class DirectoryUserMembershipForSave : EntityWithKey<int>
    {
        [NotNull]
        public int? UserId { get; set; }

        [NotNull]
        public int? DatabaseId { get; set; }
    }

    public class DirectoryUserMembership : DirectoryUserMembershipForSave
    {
        public DirectoryUser User { get; set; }

        public SqlDatabase Database { get; set; }
    }
}

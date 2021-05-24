namespace Tellma.Model.Application
{
    public static class DefStates
    {
        // Definition States
        public const string Hidden = nameof(Hidden);
        public const string Visible = nameof(Visible);
        public const string Archived = nameof(Archived);

        public static readonly string[] All = new string[] { Hidden, Visible, Archived };
    }

    public static class DefStateNames
    {
        private const string _prefix = "Definition_State_";

        public const string Hidden = _prefix + nameof(Hidden);
        public const string Visible = _prefix + nameof(Visible);
        public const string Archived = _prefix + nameof(Archived);
    }

    public static class PermissionActions
    {
        public const string Read = nameof(Read);
        public const string Update = nameof(Update);
        public const string Delete = nameof(Delete);
    }
}

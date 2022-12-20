namespace Tellma.Model.Application
{
    public static class DefStates
    {
        // Definition States
        public const string Hidden = nameof(Hidden);
        public const string Testing = nameof(Testing);
        public const string Visible = nameof(Visible);
        public const string Archived = nameof(Archived);

        public static readonly string[] All = new string[] { Hidden, Testing, Visible, Archived };
    }

    public static class DefStateNames
    {
        private const string _prefix = "Definition_State_";

        public const string Hidden = _prefix + nameof(Hidden);
        public const string Testing = _prefix + nameof(Testing);
        public const string Visible = _prefix + nameof(Visible);
        public const string Archived = _prefix + nameof(Archived);
    }
}

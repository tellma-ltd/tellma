namespace BSharp.Data
{
    /// <summary>
    /// Static class containing all buil-in views
    /// </summary>
    public static class Views
    {
        /// <summary>
        /// All the built-in views that do NOT depend on any specifications or definitions
        /// </summary>
        public static readonly ViewInfo[] BUILT_IN = new ViewInfo[]
        {
            new ViewInfo { Id = "all", Name = "View_All", Levels = new LevelInfo[] { Li("Read", false) } },
            new ViewInfo { Id = "measurement-units", Name = "MeasurementUnits", Read = true, Update = true, Delete = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "roles", Name = "Roles", Read = true, Update = true, Delete = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "users", Name = "Users", Read = true, Update = true, Delete = true, Levels = new LevelInfo[] { Li("IsActive"), Li("ResendInvitationEmail") } },
            new ViewInfo { Id = "views", Name = "Views", Read = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "ifrs-notes", Name = "IfrsNotes", Read = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "product-categories", Name = "ProductCategories", Read = true, Update = true, Delete = true, Levels = new LevelInfo[] { Li("IsActive") } },
            new ViewInfo { Id = "settings", Name = "Settings", Levels = new LevelInfo[] { Li("Read", false), Li("Update", false) } },
        };

        private static LevelInfo Li(string name, bool criteria = true)
        {
            return new LevelInfo { Action = name, Criteria = criteria };
        }

        public class ViewInfo
        {
            public string Id { get; set; }

            public string Name { get; set; }

            /// <summary>
            /// Indicates that this view is an endpoint that supports read level, both with Mask and Criteria: OData style
            /// </summary>
            public bool Read { get; set; }

            /// <summary>
            /// Indicates that this view is an endpoint that supports read level, both with Mask and Criteria: OData style
            /// </summary>
            public bool Update { get; set; }

            public bool Delete { get; set; }

            public LevelInfo[] Levels { get; set; }
        }

        public class LevelInfo
        {
            public string Action { get; set; }

            public bool Criteria { get; set; }
        }
    }
}

namespace BSharp.Data
{
    /// <summary>
    /// Static class containing all buil-in views
    /// </summary>
    public static class Views
    {
        /// <summary>
        /// All the built-in views that do NOT depend on any definitions
        /// IMPORTANT: there is a replica of this on the Client side in
        /// TS, make sure any changes here are reflected there
        /// </summary>
        public static ViewInfo[] BUILT_IN
        {
            get
            {
                return new ViewInfo[]
                {
                    new ViewInfo {
                        Id = "all",
                        Name = "View_All",
                        Actions = new ActionInfo[]
                        {
                            Li("Read", false)
                        }
                    },
                    new ViewInfo {
                        Id = "measurement-units",
                        Name = "MeasurementUnits",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "roles",
                        Name = "Roles",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "users",
                        Name = "Users",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("ResendInvitationEmail")
                        }
                    },
                    new ViewInfo {
                        Id = "ifrs-notes",
                        Name = "IfrsNotes",
                        Read = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "currencies",
                        Name = "Currencies",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "account-classifications",
                        Name = "AccountClassifications",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsDeprecated")
                        }
                    },
                    new ViewInfo {
                        Id = "accounts",
                        Name = "Accounts",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsDeprecated")
                        }
                    },
                    new ViewInfo {
                        Id = "account-types",
                        Name = "AccountTypes",
                        Actions = new ActionInfo[]
                        {
                            Li("Read", false),
                            Li("IsActive", false)
                        }
                    },
                    new ViewInfo {
                        Id = "lookup-definitions",
                        Name = "LookupDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("UpdateState")
                        }
                    },
                    new ViewInfo {
                        Id = "report-definitions",
                        Name = "ReportDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("UpdateState")
                        }
                    },
                    new ViewInfo {
                        Id = "responsibility-centers",
                        Name = "ResponsibilityCenters",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "resource-classifications",
                        Name = "ResourceClassifications",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "entry-classifications",
                        Name = "EntryClassifications",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "summary-entries",
                        Name = "SummaryEntries",
                        Read = true,
                        Actions = new ActionInfo[] {}
                    },
                    new ViewInfo {
                        Id = "settings",
                        Name = "Settings",
                        Actions = new ActionInfo[]
                        {
                            Li("Read", false),
                            Li("Update", false)
                        }
                    },
                };
            }
        }

        private static ActionInfo Li(string name, bool criteria = true)
        {
            return new ActionInfo { Action = name, Criteria = criteria };
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

            public ActionInfo[] Actions { get; set; }
        }

        public class ActionInfo
        {
            public string Action { get; set; }

            public bool Criteria { get; set; }
        }
    }
}

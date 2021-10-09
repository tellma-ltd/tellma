namespace Tellma.Data
{
    /// <summary>
    /// Static class containing all buil-in views.
    /// </summary>
    public static class Views
    {
        /// <summary>
        /// All the built-in views that do NOT depend on any definitions
        /// IMPORTANT: there is a replica of this on the Client side in
        /// TS, make sure any changes here are reflected there.
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
                        Id = "units",
                        Name = "Units",
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
                            Li("SendInvitationEmail")
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
                            Li("IsActive")
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
                            Li("IsActive")
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
                        Id = "ifrs-concepts",
                        Name = "IfrsConcepts",
                        Actions = new ActionInfo[]
                        {
                            Li("Read", false)
                        }
                    },
                    new ViewInfo {
                        Id = "report-definitions",
                        Name = "ReportDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = System.Array.Empty<ActionInfo>()
                    },
                    new ViewInfo {
                        Id = "dashboard-definitions",
                        Name = "DashboardDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = System.Array.Empty<ActionInfo>()
                    },
                    new ViewInfo {
                        Id = "centers",
                        Name = "Centers",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "entry-types",
                        Name = "EntryTypes",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive")
                        }
                    },
                    new ViewInfo {
                        Id = "exchange-rates",
                        Name = "ExchangeRates",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = System.Array.Empty<ActionInfo>()
                    },
                    new ViewInfo {
                        Id = "details-entries",
                        Name = "DetailsEntries",
                        Read = true,
                        Actions = System.Array.Empty<ActionInfo>()
                    },
                    new ViewInfo {
                        Id = "report-definitions",
                        Name = "ReportDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = System.Array.Empty<ActionInfo>()
                    },
                    new ViewInfo {
                        Id = "printing-templates",
                        Name = "PrintingTemplates",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = System.Array.Empty<ActionInfo>()
                    },
                    new ViewInfo
                    {
                        Id = "agent-definitions",
                        Name = "AgentDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("State")
                        }
                    },
                    new ViewInfo
                    {
                        Id = "resource-definitions",
                        Name = "ResourceDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("State")
                        }
                    },
                    new ViewInfo
                    {
                        Id = "lookup-definitions",
                        Name = "LookupDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("State")
                        }
                    },
                    new ViewInfo
                    {
                        Id = "line-definitions",
                        Name = "LineDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = System.Array.Empty<ActionInfo>()
                    },
                    new ViewInfo
                    {
                        Id = "document-definitions",
                        Name = "DocumentDefinitions",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("State")
                        }
                    },
                    new ViewInfo {
                        Id = "general-settings",
                        Name = "GeneralSettings",
                        Actions = new ActionInfo[]
                        {
                            Li("Read", false),
                            Li("Update", false)
                        }
                    },
                    new ViewInfo {
                        Id = "financial-settings",
                        Name = "FinancialSettings",
                        Actions = new ActionInfo[]
                        {
                            Li("Read", false),
                            Li("Update", false)
                        }
                    },
                    new ViewInfo {
                        Id = "reconciliation",
                        Name = "BankReconciliation",
                        Actions = new ActionInfo[]
                        {
                            Li("Read", false),
                            Li("Update", false)
                        }
                    },
                    new ViewInfo {
                        Id = "emails",
                        Name = "Emails",
                        Read = true,
                    },
                    new ViewInfo {
                        Id = "sms-messages",
                        Name = "SmsMessages",
                        Read = true,
                    },
                };
            }
        }



        public static ViewInfo[] ADMIN_BUILT_IN
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
                        Id = "admin-users",
                        Name = "AdminUsers",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = new ActionInfo[]
                        {
                            Li("IsActive"),
                            Li("SendInvitationEmail"),
                        }
                    },
                    new ViewInfo {
                        Id = "identity-server-users",
                        Name = "IdentityServerUsers",
                        Read = true,
                        Actions = new ActionInfo[]
                        {
                            Li("ResetPassword"),
                        }
                    },
                    new ViewInfo {
                        Id = "identity-server-clients",
                        Name = "IdentityServerClients",
                        Read = true,
                        Update = true,
                        Delete = true,
                        Actions = System.Array.Empty<ActionInfo>()
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
            /// Indicates that this view is an endpoint that supports read level with Queryex-style criteria.
            /// </summary>
            public bool Read { get; set; }

            /// <summary>
            /// Indicates that this view is an endpoint that supports read level with Queryex-style criteria.
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

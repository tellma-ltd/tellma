CREATE TYPE [dbo].[WorkflowSignatureList] AS TABLE (
	[Index]						INT				DEFAULT 0,
	[HeaderIndex]				INT				DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[RuleType]					NVARCHAR (50)	NOT NULL DEFAULT N'ByRole',
	[RuleTypeEntryIndex]		INT,
	[RoleId]					INT,
	[Userid]					INT,
	[PredicateType]				NVARCHAR(50),
	[PredicateTypeEntryIndex]	INT,
	[Value]						DECIMAL (19,4),
	[ProxyRoleId]				INT			-- If a transition has a proxy role, an agent with that proxy role can sign on behalf.
);
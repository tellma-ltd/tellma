CREATE TYPE [dbo].[WorkflowSignatureList] AS TABLE (
	[Index]			INT				DEFAULT 0,
	[HeaderIndex]	INT				DEFAULT 0,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]			INT				NOT NULL DEFAULT 0,
	-- All roles are needed to get to next positive state, one is enough to get to negative state
	[RoleId]		INT,
	[Criteria]		NVARCHAR(1024), -- when evaluated to true, the role signature becomes required
	[ProxyRoleId]	INT			-- If a transition has a proxy role, an agent with that proxy role can sign on behalf.
);
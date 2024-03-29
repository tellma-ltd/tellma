﻿CREATE TYPE [dbo].[WorkflowSignatureList] AS TABLE (
	[Index]						INT,
	[WorkflowIndex]				INT,
	[LineDefinitionIndex]		INT,
	PRIMARY KEY ([Index], [WorkflowIndex], [LineDefinitionIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[RuleType]					NVARCHAR (50),
	[RuleTypeEntryIndex]		INT,
	[RoleId]					INT,
	[UserId]					INT,
	[PredicateType]				NVARCHAR(50),
	[PredicateTypeEntryIndex]	INT,
	[Value]						DECIMAL (19,4),
	[ProxyRoleId]				INT			-- If a transition has a proxy role, an agent with that proxy role can sign on behalf.
);
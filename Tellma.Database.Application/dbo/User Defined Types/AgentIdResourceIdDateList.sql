CREATE TYPE dbo.AgentIdResourceIdDateList AS TABLE (
	[AgentId]		INT,
	[ResourceId]	INT,
	[Date]			DATE,
	PRIMARY KEY ([AgentId], [ResourceId], [Date])
);
GO
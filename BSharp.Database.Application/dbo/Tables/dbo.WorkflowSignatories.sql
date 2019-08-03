CREATE TABLE [dbo].[WorkflowSignatories] (
	[Id]			INT PRIMARY KEY,
	-- All roles are needed to get to next positive state, one is enough to get to negative state
	[RoleId]		INT,

	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	
	[RevokedAt]		DATETIMEOFFSET(7),
	[RevokedById]	INT,
	CONSTRAINT [FK_WorkflowSignatories__RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([Id]),
	CONSTRAINT [FK_WorkflowSignatories__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_WorkflowSignatories__RevokedById] FOREIGN KEY ([RevokedById]) REFERENCES [dbo].[Users] ([Id])
);
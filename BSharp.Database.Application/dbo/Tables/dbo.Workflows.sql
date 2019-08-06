CREATE TABLE [dbo].[Workflows] (
	[Id]			INT PRIMARY KEY,
	[DocumentTypeId]NVARCHAR (50)		NOT NULL,
	[FromState]		NVARCHAR (30)		NOT NULL,
	[ToState]		NVARCHAR (30)		NOT NULL,

	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	
	[RevokedAt]		DATETIMEOFFSET(7),
	[RevokedById]	INT,
	CONSTRAINT [FK_Workflows__DocumentTypes] FOREIGN KEY ([DocumentTypeId]) REFERENCES [dbo].[DocumentTypes] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [CK_Workflows__FromState] CHECK ([FromState] IN (N'Draft', N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Posted')),
	CONSTRAINT [CK_Workflows__ToState] CHECK ([ToState] IN (N'Draft', N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Posted')),
	CONSTRAINT [FK_Workflows__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_Workflows__RevokedById] FOREIGN KEY ([RevokedById]) REFERENCES [dbo].[Users] ([Id])
);
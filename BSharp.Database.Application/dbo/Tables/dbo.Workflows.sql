CREATE TABLE [dbo].[Workflows] (
	[Id]			INT					CONSTRAINT [PK_Workflows] PRIMARY KEY IDENTITY,
	[DocumentTypeId]NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Workflows__DocumentTypes] FOREIGN KEY ([DocumentTypeId]) REFERENCES [dbo].[DocumentTypes] ([Id]) ON DELETE CASCADE,
	-- Must be a positive state
	[FromState]		NVARCHAR (30)		NOT NULL CONSTRAINT [CK_Workflows__FromState] CHECK ([FromState] IN (N'Draft', N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Posted')),
	-- Must be a positive state
	[ToState]		NVARCHAR (30)		NOT NULL CONSTRAINT [CK_Workflows__ToState] CHECK ([ToState] IN (N'Draft', N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Posted')),

	[IsPaperless]	BIT					NOT NULL DEFAULT 1, -- When 0, user can specify who signed on paper.			
	[CreatedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]	INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Workflows__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	
	-- To be moved to WorkflowsAudit
	--[RevokedAt]		DATETIMEOFFSET(7),
	--[RevokedById]	INT CONSTRAINT [FK_Workflows__RevokedById] FOREIGN KEY ([RevokedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE INDEX [IX_Workflows__DocumentTypeId_FromState] ON dbo.Workflows([DocumentTypeId], [FromState]) ; --WHERE [RevokedById] IS NULL;
GO
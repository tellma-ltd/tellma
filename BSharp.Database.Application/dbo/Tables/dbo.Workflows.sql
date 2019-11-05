CREATE TABLE [dbo].[Workflows] (
	[Id]				INT					CONSTRAINT [PK_Workflows] PRIMARY KEY IDENTITY,
	[LineDefinitionId]	NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Workflows__LineDefinitions] FOREIGN KEY ([LineDefinitionId]) REFERENCES [dbo].[LineDefinitions] ([Id]) ON DELETE CASCADE,
	-- Must be a positive state
	[FromState]			NVARCHAR (30)		NOT NULL CONSTRAINT [CK_Workflows__FromState] CHECK ([FromState] IN (N'Draft', N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Reviewed')),
	-- Must be a positive state
	[ToState]			NVARCHAR (30)		NOT NULL CONSTRAINT [CK_Workflows__ToState] CHECK ([ToState] IN (N'Draft', N'Void', N'Requested', N'Rejected', N'Authorized', N'Failed', N'Completed', N'Invalid', N'Reviewed')),		
	--[SavedAt]			AS [ValidFrom] AT TIME ZONE 'UTC',
	[SavedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Workflows__SavedById] FOREIGN KEY ([SavedById]) REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[WorkflowsHistory]));
GO
CREATE UNIQUE INDEX [IX_Workflows__LineDefinitionId_FromState] ON dbo.Workflows([LineDefinitionId], [FromState]) ; --WHERE [RevokedById] IS NULL;
GO
CREATE TABLE [dbo].[Workflows] (
	[Id]				INT					CONSTRAINT [PK_Workflows] PRIMARY KEY IDENTITY,
	[LineDefinitionId]	NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Workflows__LineDefinitions] REFERENCES [dbo].[LineDefinitions] ([Id]) ON DELETE CASCADE,
	-- Must be a positive state
	[FromState]			SMALLINT			NOT NULL CONSTRAINT [CK_Workflows__FromState] CHECK ([FromState] >= 0),
	-- Must be a positive state
	[ToState]			SMALLINT			NOT NULL CONSTRAINT [CK_Workflows__ToState] CHECK ([ToState] > 0),
	--[SavedAt]			AS [ValidFrom] AT TIME ZONE 'UTC',
	[SavedById]			INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Workflows__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2			GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2			GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[WorkflowsHistory]));
GO
CREATE UNIQUE INDEX [IX_Workflows__LineDefinitionId_FromState] ON dbo.Workflows([LineDefinitionId], [FromState]) ; --WHERE [RevokedById] IS NULL;
GO
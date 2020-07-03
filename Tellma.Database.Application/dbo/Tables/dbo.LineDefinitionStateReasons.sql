CREATE TABLE [dbo].[LineDefinitionStateReasons]
(
	[Id]				INT				CONSTRAINT [PK_LineDefinitionStateReasons] PRIMARY KEY IDENTITY,
	[LineDefinitionId]	INT				NOT NULL CONSTRAINT [FK_LineDefinitionStateReasons__LineDefinitionId] REFERENCES dbo.LineDefinitions([Id]),
	[State]				SMALLINT		NOT NULL,
	[Name]				NVARCHAR (50)	NOT NULL,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[IsActive]			BIT				NOT NULL DEFAULT 1,
	[SavedById]			INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionStateReasons__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]			DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]			DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionStateReasonsHistory]));
GO;
CREATE TABLE [dbo].[LineDefinitionGenerateParameters]
(
	[Id]						INT				CONSTRAINT [PK_LineDefinitionGenerateParameters] PRIMARY KEY IDENTITY,
	[LineDefinitionId]			INT				NOT NULL 
	CONSTRAINT [FK_LineDefinitionGenerateParameters__LineDefinitionId] REFERENCES [dbo].[LineDefinitions] ([Id]) ON DELETE CASCADE,
	[Index]						INT				NOT NULL,
	CONSTRAINT [IX_LineDefinitionGenerateParameters] UNIQUE ([LineDefinitionId], [Index]),
	[Key]						NVARCHAR (50)	NOT NULL,
	[Label]						NVARCHAR (50)	NOT NULL,
	[Label2]					NVARCHAR (50),
	[Label3]					NVARCHAR (50),
	[Visibility]				NVARCHAR (50)	NOT NULL, -- N'None', N'Optional', N'Required'
	[Control]					NVARCHAR (50)	NOT NULL, -- 'text', 'number', 'decimal', 'date', 'boolean', 'Resource'
	[ControlOptions]			NVARCHAR (1024),
	[SavedById]					INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionGenerateParameters__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[LineDefinitionGenerateParametersHistory]));
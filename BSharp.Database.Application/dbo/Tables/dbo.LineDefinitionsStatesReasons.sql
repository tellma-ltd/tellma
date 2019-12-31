CREATE TABLE [dbo].[LineDefinitionsStatesReasons]
(
	[Id]				INT				CONSTRAINT [PK_LineDefinitionsStatesReasons] PRIMARY KEY IDENTITY,
	[LineDefinitionId]	NVARCHAR (50)	NOT NULL,
	[StateId]			SMALLINT		NOT NULL,
	[Name]				NVARCHAR (50)	NOT NULL,
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[IsActive]			BIT				DEFAULT 1,
	[CreatedAt]			DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineStatesReasons__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]		INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineStatesReasons__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
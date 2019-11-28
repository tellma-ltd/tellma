CREATE TABLE [dbo].[AgentDefinitions]
(
	[Id]				NVARCHAR(50) PRIMARY KEY,
	[TitleSingular]		NVARCHAR (255),
	[TitleSingular2]	NVARCHAR (255),
	[TitleSingular3]	NVARCHAR (255),
	[TitlePlural]		NVARCHAR (255),
	[TitlePlural2]		NVARCHAR (255),
	[TitlePlural3]		NVARCHAR (255),

	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AgentDefinitions__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_AgentDefinitions__ModifiedById]  FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
)

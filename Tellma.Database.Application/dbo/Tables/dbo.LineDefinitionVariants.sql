CREATE TABLE [dbo].[LineDefinitionVariants]
(
	[Id]					INT CONSTRAINT [PK_LineDefinitionOptions] PRIMARY KEY IDENTITY,
	[LineDefinitionId]		INT NOT NULL CONSTRAINT [FK_LineDefinitionVariants__LineDefinitionId] REFERENCES dbo.LineDefinitions([Id]),
	[Index]					TINYINT	NOT NULL,
	CONSTRAINT [UX_LineDefinitionVariants__LineDefinitionId_Index] UNIQUE ([LineDefinitionId], [Index]),
	[Name]					NVARCHAR (50) NOT NULL,
	[Name2]					NVARCHAR (50),
	[Name3]					NVARCHAR (50),
	[SavedAt]				DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[SavedById]				INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_LineDefinitionVariants__SavedById] REFERENCES [dbo].[Users] ([Id])
);
CREATE TABLE [dbo].[Segments] (
	[Id]				INT					CONSTRAINT [PK_Segments] PRIMARY KEY NONCLUSTERED IDENTITY,
	[Name]				NVARCHAR (255)		NOT NULL, -- appears in select lists
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[IsActive]			BIT					NOT NULL DEFAULT 1,
	[Code]				NVARCHAR(10), -- code for import
	[SortKey]			DECIMAL (9,4),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Segments__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Segments__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE CLUSTERED INDEX  [IX_Segments__SortKey] ON [dbo].[Segments]([SortKey])	
CREATE TABLE [dbo].[ResourceLookup1s] (
	[Id]				INT	PRIMARY KEY NONCLUSTERED,
	[Name]				NVARCHAR (255)		NOT NULL, -- appears in select lists
	[Name2]				NVARCHAR (255),
	[Name3]				NVARCHAR (255),
	[IsActive]			BIT					NOT NULL DEFAULT 1,
	[IsDeleted]			BIT					NOT NULL DEFAULT 0,
	[SortKey]			DECIMAL (9,4),	-- Sort code for reporting purposes
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')),

	CONSTRAINT [FK_ResourceLookup1s__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [FK_ResourceLookup1s__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE CLUSTERED INDEX [IX_ResourceLookup1s__SortKey]
  ON [dbo].[ResourceLookup1s]([SortKey])
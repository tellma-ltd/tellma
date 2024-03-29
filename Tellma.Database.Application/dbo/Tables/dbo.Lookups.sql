﻿CREATE TABLE [dbo].[Lookups] (
	[Id]				INT					CONSTRAINT [PK_Lookups] PRIMARY KEY NONCLUSTERED IDENTITY,
	[DefinitionId]		INT					NOT NULL CONSTRAINT [FK_Lookups__DefinitionId] REFERENCES dbo.LookupDefinitions([Id]),
	[Name]				NVARCHAR (50)		NOT NULL, -- appears in select lists
	[Name2]				NVARCHAR (50),
	[Name3]				NVARCHAR (50),
	[IsActive]			BIT					NOT NULL DEFAULT 1,
	[Code]				NVARCHAR(10), -- code for inter-tenant reporting
	[SortKey]			DECIMAL (9,4),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL CONSTRAINT [FK_Lookups__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL CONSTRAINT [FK_Lookups__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE CLUSTERED INDEX  [IX_Lookups__SortKey] ON [dbo].[Lookups]([DefinitionId], [SortKey])	-- Sort code for reporting purposes
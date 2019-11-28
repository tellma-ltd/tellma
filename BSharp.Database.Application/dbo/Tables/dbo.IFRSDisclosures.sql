CREATE TABLE [dbo].[IfrsDisclosures] (
	[IfrsDisclosureId]	NVARCHAR (255) CONSTRAINT [FK_IfrsDisclosures__IfrsDisclosureId] FOREIGN KEY ([IfrsDisclosureId]) REFERENCES [dbo].[IfrsConcepts] ([Id]),
	[Concept]			NVARCHAR (255) CONSTRAINT [FK_IfrsDisclosures__Concept] FOREIGN KEY ([Concept]) REFERENCES [dbo].[IfrsConcepts] ([Id]),
	CONSTRAINT [PK_IfrsDisclosures__IfrsDisclosureId_Concept] PRIMARY KEY ([IfrsDisclosureId], [Concept]),
	[IsActive]			BIT DEFAULT 1,
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosures__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosures__ModifiedById]  FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
)
GO
CREATE TABLE [dbo].[IfrsDisclosures] (
	[Id]				INT PRIMARY KEY IDENTITY,
	[IfrsDisclosureId]	NVARCHAR (255) CONSTRAINT [FK_IfrsDisclosures__IfrsDisclosureId] REFERENCES [dbo].[IfrsConcepts] ([Code]),
	[Concept]			NVARCHAR (255) CONSTRAINT [FK_IfrsDisclosures__Concept] REFERENCES [dbo].[IfrsConcepts] ([Code]),
	CONSTRAINT [UQ_IfrsDisclosures__IfrsDisclosureId_Concept] UNIQUE ([IfrsDisclosureId], [Concept]),
	[IsActive]			BIT DEFAULT 1,
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosures__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]		INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosures__ModifiedById] REFERENCES [dbo].[Users] ([Id])
)
GO
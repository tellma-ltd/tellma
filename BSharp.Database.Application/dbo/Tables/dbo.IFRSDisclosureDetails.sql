CREATE TABLE [dbo].[IfrsDisclosureDetails] (
	[IfrsDisclosureId]	NVARCHAR (255),--		CONSTRAINT [FK_IfrsDisclosureDetails__IfrsDisclosureId] REFERENCES [dbo].[IfrsDisclosures] ([IfrsDisclosureId]) ON DELETE CASCADE,
	[Concept]			NVARCHAR (255),--		CONSTRAINT [FK_IfrsDisclosureDetails__Concept] REFERENCES [dbo].[IfrsConcepts] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_IfrsDisclosureDetails__IfrsDisclosureId_Concept] FOREIGN KEY ([IfrsDisclosureId], [Concept])	REFERENCES [dbo].[IfrsDisclosures] ([IfrsDisclosureId], [Concept]) ON DELETE CASCADE,
	[ValidSince]		Date				DEFAULT '0001.01.01',
	[Value]				NVARCHAR (255),
	CONSTRAINT [PK_IfrsDisclosureDetails__IfrsDisclosureId_Concept_ValidSince_Value] PRIMARY KEY ([IfrsDisclosureId], [Concept], [ValidSince], [Value]),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosureDetails__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosureDetails__ModifiedById] REFERENCES [dbo].[Users] ([Id]),
);
GO
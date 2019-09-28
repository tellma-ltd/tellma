CREATE TABLE [dbo].[IfrsDisclosureDetails] (
	[IfrsDisclosureId]	NVARCHAR (255),--		CONSTRAINT [FK_IfrsDisclosureDetails__IfrsDisclosureId]	FOREIGN KEY ([IfrsDisclosureId])	REFERENCES [dbo].[IfrsDisclosures] ([IfrsDisclosureId]) ON DELETE CASCADE,
	[Concept]			NVARCHAR (255),--		CONSTRAINT [FK_IfrsDisclosureDetails__Concept]	FOREIGN KEY ([Concept])	REFERENCES [dbo].[IfrsConcepts] ([Id]) ON DELETE CASCADE,
	[ValidSince]		Date				DEFAULT '0001.01.01',
	CONSTRAINT [PK_IfrsDisclosureDetails__IfrsDisclosureId_Concept_ValidSince] PRIMARY KEY ([IfrsDisclosureId], [Concept], [ValidSince]),
	CONSTRAINT [FK_IfrsDisclosureDetails__IfrsDisclosureId_Concept] FOREIGN KEY ([IfrsDisclosureId], [Concept])	REFERENCES [dbo].[IfrsDisclosures] ([IfrsDisclosureId], [Concept]) ON DELETE CASCADE,
	[Value]				NVARCHAR (255),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosureDetails__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosureDetails__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
);
GO
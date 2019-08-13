CREATE TABLE [dbo].[IfrsDisclosureDetails] (
	[IfrsDisclosureId]	NVARCHAR (255)		NOT NULL CONSTRAINT [FK_IfrsDisclosureDetails__IfrsDisclosures]	FOREIGN KEY ([IfrsDisclosureId])	REFERENCES [dbo].[IfrsDisclosures] ([Id]) ON DELETE CASCADE,
	[ValidSince]		Date				NOT NULL DEFAULT '0001.01.01',
	[Value]				NVARCHAR (255),
	[CreatedAt]			DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosureDetails__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]		DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]		INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_IfrsDisclosureDetails__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [PK_IfrsDisclosureDetails__IfrsDisclosureId_ValidSince] PRIMARY KEY ([IfrsDisclosureId], [ValidSince])	
);
GO
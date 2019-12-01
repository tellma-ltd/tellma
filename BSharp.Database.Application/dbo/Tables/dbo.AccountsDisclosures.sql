CREATE TABLE [dbo].[AccountsDisclosures] ( -- TODO: change the table name
-- We map EntryId to a Concept in IfrsDisclouse Id
	[IfrsDisclosureId]				NVARCHAR (255), -- StatementOf__Abstract and disclosure notes from 800500
	[AccountTypeId]					NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Disclosures__AccountTypeId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[AgentDefinitionId]				NVARCHAR (50),
	[ResourceClassificationCode]	NVARCHAR (255),
	[IsCurrent]						BIT					NOT NULL DEFAULT 1,
	[EntryClassificationCode]		NVARCHAR (255),
	[Concept]						NVARCHAR (255) NOT NULL, -- the taxonomy defines whether to use instant or period.
	CONSTRAINT [PK_AccountsDisclosures] UNIQUE ([IfrsDisclosureId], [AccountTypeId], [AgentDefinitionId], [ResourceClassificationCode], [IsCurrent], [EntryClassificationCode])
);
CREATE TABLE [dbo].[AccountsDisclosures] ( -- TODO: change the table name
-- We map EntryId to a Concept in IfrsDisclouse Id
	[IfrsDisclosureId]				NVARCHAR (255), -- StatementOf__Abstract and disclosure notes from 800500
	[AgentDefinitionId]				NVARCHAR (50),
	[IfrsTypeId]					INT,
	--[IsCurrent]						BIT					NOT NULL DEFAULT 1,
	[EntryTypeId]					INT,
	[Concept]						NVARCHAR (255) NOT NULL, -- the taxonomy defines whether to use instant or period.
	CONSTRAINT [PK_AccountsDisclosures] UNIQUE ([IfrsDisclosureId], [AgentDefinitionId], [IfrsTypeId], [EntryTypeId])
);
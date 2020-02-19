CREATE TABLE [dbo].[DocumentDefinitions] (
-- table managed by Banan
-- Note that, in steel production: CTS, HSP, and SM are considered 3 different document types.
	[Id]						NVARCHAR (50)	CONSTRAINT [PK_DocumentDefinitions] PRIMARY KEY,
	-- IsPrimal, means that we are not copying the data from elsewhere. Instead, this is the only place where it exists
	-- Original is less confusing than Source Document. An SIV is not a source document, but can be primal
	[IsOriginalDocument]		BIT				DEFAULT 1, -- <=> IsVoucherReferenceRequired = 0
	-- EvidenceType = Authentication <=> Document is paperless. Workflow and Account signatures are required
	-- EvidenceType = SourceDocument <=> There is a external booklet from which we are copying. In that case, Include voucher booklet and reference. Only workflow required
	-- EvidenceType = Attachment <=> There is no external booklet. Instead, there are support documents proving what happened. In that case, attach them. Only workflow required
	-- EvidenceType = Trust <=> There is no supporting document proving what happened. Simply accept the posting as is. Only workflow required
	--[EvidenceTypeId]			NVARCHAR (30)	NOT NULL DEFAULT N'Trust' CONSTRAINT [CK_DocumentDefinitions__EvidenceTypeId] CHECK ([EvidenceTypeId] IN (N'Authentication', N'SourceDocument', N'Attachment', N'Trust')),
	[TitleSingular]				NVARCHAR (255),
	[TitleSingular2]			NVARCHAR (255),
	[TitleSingular3]			NVARCHAR (255),
	[TitlePlural]				NVARCHAR (255),
	[TitlePlural2]				NVARCHAR (255),
	[TitlePlural3]				NVARCHAR (255),

	[IsImmutable]				BIT				NOT NULL DEFAULT 0, -- 1 <=> Cannot change without invalidating signatures
	-- UI Specs
	[SortKey]					DECIMAL (9,4),
	[Prefix]					NVARCHAR (5)	NOT NULL,
	[CodeWidth]					TINYINT			DEFAULT 3, -- For presentation purposes

	[AgentDefinitionList]		NVARCHAR (1024),

	[State]						NVARCHAR (50)			DEFAULT N'Draft',	-- Deployed, Archived (Phased Out)
	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),			-- IF Null, it does not show on the main menu
	[MainMenuSortKey]			DECIMAL (9,4),
	[SavedById]					INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentDefinitions__SavedById] REFERENCES [dbo].[Users] ([Id]),
	[ValidFrom]					DATETIME2		GENERATED ALWAYS AS ROW START NOT NULL,
	[ValidTo]					DATETIME2		GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
	PERIOD FOR SYSTEM_TIME ([ValidFrom], [ValidTo])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.[DocumentDefinitionsHistory]));
GO;
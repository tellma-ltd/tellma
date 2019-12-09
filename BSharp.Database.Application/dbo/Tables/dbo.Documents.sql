CREATE TABLE [dbo].[Documents] (
--	This table for all business documents that are routed for requisition, authorization, completion, and posting.
--	Its scope is

-- Kimbirly suggestion: [Id]: PRIMARY KEY NONCLUSTERED, ([DocumentDate], [Id]): Clustered index
	[Id]							INT				CONSTRAINT [PK_Documents] PRIMARY KEY IDENTITY,
	-- Common to all document types
	[DefinitionId]					NVARCHAR (50)	NOT NULL CONSTRAINT [FK_Documents__DefinitionId] REFERENCES [dbo].[DocumentDefinitions] ([Id]) ON UPDATE CASCADE,
	[SerialNumber]					INT				NOT NULL,	-- auto generated, copied to paper if needed.
	[OperatingSegmentId]			INT				NOT NULL CONSTRAINT [FK_Documents__OperatingSegmentId] REFERENCES dbo.ResponsibilityCenters([Id]),
	[DocumentDate]					DATE			NOT NULL DEFAULT CONVERT (DATE, SYSDATETIME()) CONSTRAINT [CK_Documents__DocumentDate] CHECK ([DocumentDate] < DATEADD(DAY, 1, GETDATE())),
	[State]							SMALLINT		NOT NULL DEFAULT 0,
	
	-- For a source socument, Evidence type == Authentication. Else source document, Attachment, trust
	-- When evidence type = source document
	[VoucherBookletId]				INT, -- each range might be dedicated for a special purpose
	[VoucherNumericReference]		INT, -- must fall between RangeStarts and RangeEnds of the booklet
	-- Dynamic properties defined by document type specification
	[DocumentLookup1Id]				INT, -- e.g., cash machine serial in the case of a sale
	[DocumentLookup2Id]				INT,
	[DocumentLookup3Id]				INT,
	[DocumentText1]					NVARCHAR (255),
	[DocumentText2]					NVARCHAR (255),
	--[SortKey]						DECIMAL (9,4)	NOT NULL,
	-- Additional properties to simplify data entry. No report should be based on them!!!
	[Memo]							NVARCHAR (255),
	[MemoIsCommon]					BIT				DEFAULT 1,
	--[AgentId]						INT,
	--[CurrencyId]					INT, 
	--[InvoiceReference]				NVARCHAR (50),
	-- Transaction specific, to record the acquisition or loss of goods and services
	-- Orders that are not negotiables, are assumed to happen, and hence are journalized, even we are verifying it later.
	-- an easy way to define a recurrent document	
	[Frequency]						NVARCHAR (30)	NOT NULL DEFAULT (N'OneTime') CONSTRAINT [CK_Documents__Frequency] CHECK ([Frequency] IN (N'OneTime', N'Daily', N'Weekly', N'Monthly', N'Quarterly', N'Yearly')),
	[Repetitions]					INT				NOT NULL DEFAULT 0, -- time unit is function of frequency
	--[EndDate] AS (
	--				CASE 
	--					WHEN [Frequency] = N'OneTime' THEN [DocumentDate]
	--					WHEN [Frequency] = N'Daily' THEN DATEADD(DAY, [Repetitions], [DocumentDate])
	--					WHEN [Frequency] = N'Weekly' THEN DATEADD(WEEK, [Repetitions], [DocumentDate])
	--					WHEN [Frequency] = N'Monthly' THEN DATEADD(MONTH, [Repetitions], [DocumentDate])
	--					WHEN [Frequency] = N'Quarterly' THEN DATEADD(QUARTER, [Repetitions], [DocumentDate])
	--					WHEN [Frequency] = N'Yearly' THEN DATEADD(YEAR, [Repetitions], [DocumentDate])
	--				END
	--) PERSISTED,
	-- Entered while the lines are still in the requested state.
	[Request]						NVARCHAR (1024), --  this the detailed explanation
	[NeededBy]						DATE, -- here or in lines (?)
	[Response]						NVARCHAR (1024),
	-- Offer expiry date can be put on the generated template (expires in two weeks from above date)
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Documents__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]					INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Documents__ModifiedById]  FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
	-- If the company is in Alofi, and the server is hosted in Apia, the server time will be one day behind
	-- So, the user will not be able to enter transactions unless DocumentDate is allowed 1d future 	
 );
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Documents__VoucherBooklet_VoucherNumericReference]
  ON [dbo].[Documents]([VoucherBookletId], [VoucherNumericReference])
  WHERE [VoucherNumericReference] IS NOT NULL;
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Documents__DocumentDefinitionId_SerialNumber]
  ON [dbo].[Documents]([DefinitionId], [SerialNumber])
GO
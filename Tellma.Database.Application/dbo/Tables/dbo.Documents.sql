CREATE TABLE [dbo].[Documents] (
--	This table for all business documents that are routed for requisition, authorization, completion, and posting.
--	Its scope is

-- Kimbirly suggestion: [Id]: PRIMARY KEY NONCLUSTERED, ([PostingDate], [Id]): Clustered index
	[Id]							INT				CONSTRAINT [PK_Documents] PRIMARY KEY IDENTITY,
	-- Common to all document types
	[DefinitionId]					INT				NOT NULL CONSTRAINT [FK_Documents__DefinitionId] REFERENCES [dbo].[DocumentDefinitions] ([Id]),
	[SerialNumber]					INT				NOT NULL,	-- auto generated, copied to paper if needed.
	CONSTRAINT [IX_Documents__DocumentDefinitionId_SerialNumber] UNIQUE ([DefinitionId], [SerialNumber]),
	[State]							SMALLINT		NOT NULL DEFAULT 0 CONSTRAINT [CK_Documents__State] CHECK ([State] BETWEEN -1 AND +1),
	[StateAt]						DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[PostingDate]					DATE			CONSTRAINT [CK_Documents__PostingDate] CHECK ([PostingDate] < DATEADD(DAY, 1, GETDATE())),
	CONSTRAINT [Documents__PostingDate_State] CHECK([State] < 1 OR [PostingDate] IS NOT NULL),
	[Clearance]						TINYINT			NOT NULL DEFAULT 0 CONSTRAINT [CK_Documents__Clearance] CHECK ([Clearance] BETWEEN 0 AND 2),
	-- Dynamic properties defined by document type specification
	[DocumentLookup1Id]				INT, -- e.g., cash machine serial in the case of a sale
	[DocumentLookup2Id]				INT,
	[DocumentLookup3Id]				INT,
	[DocumentText1]					NVARCHAR (255),
	[DocumentText2]					NVARCHAR (255),
	-- Additional properties to simplify data entry. No report should be based on them!!!
	[Memo]							NVARCHAR (255),
	[MemoIsCommon]					BIT				NOT NULL DEFAULT 1,
	-- Agent Definition is specified in DocumentDefinition
	[DebitRelationId]				INT	CONSTRAINT [FK_Documents__DebitRelationId] REFERENCES dbo.[Relations]([Id]), 
	[DebitRelationIsCommon]			BIT				NOT NULL DEFAULT 0,
	[CreditRelationId]				INT	CONSTRAINT [FK_Documents__CreditRelationId] REFERENCES dbo.[Relations]([Id]), 
	[CreditRelationIsCommon]		BIT				NOT NULL DEFAULT 0,
	[NotedRelationId]				INT	CONSTRAINT [FK_Documents__NotedRelationId] REFERENCES dbo.[Relations]([Id]), 
	[NotedRelationIsCommon]			BIT				NOT NULL DEFAULT 0,
	[InvestmentCenterId]			INT,
	[InvestmentCenterIsCommon]		BIT				NOT NULL DEFAULT 1,
	[Time1]							DATETIME2 (2),
	[Time1IsCommon]					BIT				NOT NULL DEFAULT 0,
	[Time2]							DATETIME2 (2),
	[Time2IsCommon]					BIT				NOT NULL DEFAULT 0,
	[Quantity]						DECIMAL (19,4)	NULL,
	[QuantityIsCommon]				BIT				NOT NULL DEFAULT 0,
	[UnitId]						INT CONSTRAINT [FK_Documents__UnitId] REFERENCES dbo.[Units]([Id]),
	[UnitIsCommon]					BIT				NOT NULL DEFAULT 0,
	[CurrencyId]					NCHAR (3) CONSTRAINT [FK_Documents__CurrencyId] REFERENCES dbo.Currencies([Id]),
	[CurrencyIsCommon]				BIT				NOT NULL DEFAULT 0,
	-- Offer expiry date can be put on the generated template (expires in two weeks from above date)
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Documents__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(), 
	[ModifiedById]					INT	NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Documents__ModifiedById] REFERENCES [dbo].[Users] ([Id])
	-- If the company is in Alofi, and the server is hosted in Apia, the server time will be one day behind
	-- So, the user will not be able to enter transactions unless PostingDate is allowed 1d future 	
 );
GO
CREATE TRIGGER TRG_Documents__State ON dbo.Documents
FOR UPDATE
AS
IF UPDATE([State])
	INSERT INTO dbo.DocumentStatesHistory([DocumentId], [FromState], [ToState])
	SELECT I.[Id], D.[State], I.[State]
	FROM INSERTED I 
	JOIN DELETED D ON I.[Id] = D.[Id]
	WHERE D.[State] <> I.[State];
GO
/*
 * Marmin (UAE) e-invoicing -- schema migration.
 *
 * GENERATED FILE. Do not edit by hand: run tools/Migrations/generate-marmin-ae-migration.py,
 * which copies every object body verbatim out of Tellma.Database.Application so that this script
 * and the database project cannot drift apart.
 *
 * Safe to run more than once: the column additions are guarded, and every programmable object is
 * CREATE OR ALTER.
 *
 * BEFORE RUNNING
 *   1. Take a backup. Section 2 drops and recreates the dbo.DocumentDefinitionList table type,
 *      and a table type cannot be ALTERed, so its three dependent procedures must be dropped and
 *      recreated around it. A single transaction cannot span the GO batches that CREATE PROCEDURE
 *      requires, so a failure part-way through leaves those three procedures missing.
 *   2. Run it in a maintenance window. Between the first and last batch of section 2, saving a
 *      document definition fails.
 */
SET NOCOUNT ON;
GO

-- Required, not cosmetic. sqlcmd defaults QUOTED_IDENTIFIER to OFF, and SQL Server refuses to
-- create a filtered index (section 1) or to compile a stored procedure that later touches one
-- unless it is ON. SSDT sets both of these for the same reason.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '--- 1. Columns and indexes -------------------------------------------------';
GO
IF COL_LENGTH('dbo.Settings', 'MarminAeEnvironment') IS NULL
    ALTER TABLE [dbo].[Settings] ADD [MarminAeEnvironment] NVARCHAR(10) NOT NULL CONSTRAINT [DF_Settings__MarminAeEnvironment] DEFAULT N'Sandbox';
GO
IF COL_LENGTH('dbo.Settings', 'MarminAeEncryptedClientSecret') IS NULL
    ALTER TABLE [dbo].[Settings] ADD [MarminAeEncryptedClientSecret] NVARCHAR(MAX) NULL;
GO
IF COL_LENGTH('dbo.Settings', 'MarminAeEncryptedWebhookSecret') IS NULL
    ALTER TABLE [dbo].[Settings] ADD [MarminAeEncryptedWebhookSecret] NVARCHAR(MAX) NULL;
GO
IF COL_LENGTH('dbo.Settings', 'MarminAeEncryptionKeyIndex') IS NULL
    ALTER TABLE [dbo].[Settings] ADD [MarminAeEncryptionKeyIndex] INT NOT NULL CONSTRAINT [DF_Settings__MarminAeEncryptionKeyIndex] DEFAULT 0;
GO
IF COL_LENGTH('dbo.Documents', 'MarminAeState') IS NULL
    ALTER TABLE [dbo].[Documents] ADD [MarminAeState] INT NULL;
GO
IF COL_LENGTH('dbo.Documents', 'MarminAeDocumentId') IS NULL
    ALTER TABLE [dbo].[Documents] ADD [MarminAeDocumentId] NVARCHAR(50) NULL;
GO
IF COL_LENGTH('dbo.Documents', 'MarminAeDocumentNumber') IS NULL
    ALTER TABLE [dbo].[Documents] ADD [MarminAeDocumentNumber] NVARCHAR(50) NULL;
GO
IF COL_LENGTH('dbo.Documents', 'MarminAeResult') IS NULL
    ALTER TABLE [dbo].[Documents] ADD [MarminAeResult] NVARCHAR(MAX) NULL;
GO
IF COL_LENGTH('dbo.Documents', 'MarminAeLastEventId') IS NULL
    ALTER TABLE [dbo].[Documents] ADD [MarminAeLastEventId] UNIQUEIDENTIFIER NULL;
GO
IF COL_LENGTH('dbo.Documents', 'MarminAeLastEventAt') IS NULL
    ALTER TABLE [dbo].[Documents] ADD [MarminAeLastEventAt] DATETIMEOFFSET(7) NULL;
GO
IF COL_LENGTH('dbo.DocumentDefinitions', 'MarminAeDocumentType') IS NULL
    ALTER TABLE [dbo].[DocumentDefinitions] ADD [MarminAeDocumentType] NVARCHAR(20) NULL CONSTRAINT [CK_DocumentDefinitions__MarminAeDocumentType] CHECK ([MarminAeDocumentType] IN (N'SalesInvoice', N'SalesCreditNote'));
GO
IF COL_LENGTH('dbo.DocumentDefinitions', 'MarminAeTypeCode') IS NULL
    ALTER TABLE [dbo].[DocumentDefinitions] ADD [MarminAeTypeCode] NVARCHAR(10) NULL;
GO
IF OBJECT_ID(N'[dbo].[CK_Settings__MarminAeEnvironment]') IS NULL
    ALTER TABLE [dbo].[Settings] ADD CONSTRAINT [CK_Settings__MarminAeEnvironment] CHECK ([MarminAeEnvironment] IN (N'Sandbox', N'Production'));
GO
IF INDEXPROPERTY(OBJECT_ID('dbo.Documents'), 'IX_Documents__MarminAeDocumentId', 'IndexID') IS NULL
    CREATE NONCLUSTERED INDEX [IX_Documents__MarminAeDocumentId]
      ON [dbo].[Documents]([MarminAeDocumentId]) WHERE [MarminAeDocumentId] IS NOT NULL;
GO

PRINT '--- 2. dbo.DocumentDefinitionList table type --------------------------------';
GO
DROP PROCEDURE IF EXISTS [api].[DocumentDefinitions__Save];
GO
DROP PROCEDURE IF EXISTS [bll].[DocumentDefinitions_Validate__Save];
GO
DROP PROCEDURE IF EXISTS [dal].[DocumentDefinitions__Save];
GO
DROP TYPE IF EXISTS [dbo].[DocumentDefinitionList];
GO
CREATE TYPE [dbo].[DocumentDefinitionList] AS TABLE (
	[Index]						INT	PRIMARY KEY,
	[Id]						INT NOT NULL DEFAULT 0,
	[Code]						NVARCHAR (50),
	[IsOriginalDocument]		BIT,
	[Description]				NVARCHAR (1024),
	[Description2]				NVARCHAR (1024),
	[Description3]				NVARCHAR (1024),
	[TitleSingular]				NVARCHAR (50),
	[TitleSingular2]			NVARCHAR (50),
	[TitleSingular3]			NVARCHAR (50),
	[TitlePlural]				NVARCHAR (50),
	[TitlePlural2]				NVARCHAR (50),
	[TitlePlural3]				NVARCHAR (50),

	-- UI Specs
	[Prefix]					NVARCHAR (5),
	[CodeWidth]					TINYINT, -- For presentation purposes
	
	[PostingDateVisibility]		NVARCHAR (50),
	[CenterVisibility]			NVARCHAR (50),
	
	[Lookup1Label]						NVARCHAR (50),
	[Lookup1Label2]						NVARCHAR (50),
	[Lookup1Label3]						NVARCHAR (50),
	[Lookup1Visibility]					NVARCHAR (50),
	[Lookup1DefinitionId]				INT,
	[Lookup2Label]						NVARCHAR (50),
	[Lookup2Label2]						NVARCHAR (50),
	[Lookup2Label3]						NVARCHAR (50),
	[Lookup2Visibility]					NVARCHAR (50),
	[Lookup2DefinitionId]				INT,

	[ZatcaDocumentType]			NVARCHAR (3), -- 381, 383, 388, 389

	[MarminAeDocumentType]		NVARCHAR (20), -- SalesInvoice, SalesCreditNote
	[MarminAeTypeCode]			NVARCHAR (10), -- The literal invoice_type_code / credit_note_type_code

	[ClearanceVisibility]		NVARCHAR (50),
	[MemoVisibility]			NVARCHAR (50),

	[AttachmentVisibility]		NVARCHAR (50),
	[HasBookkeeping]			BIT,
	[CloseValidateScript]		NVARCHAR (MAX),

	[MainMenuIcon]				NVARCHAR (50),
	[MainMenuSection]			NVARCHAR (50),	-- IF Null, it does not show on the main menu
	[MainMenuSortKey]			DECIMAL (9,4)
);
GO

CREATE OR ALTER PROCEDURE [api].[DocumentDefinitions__Save]
	@Entities [DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50) = N'en',
	@NeutralCulture NVARCHAR(50) = N'en'
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	-- (1) Validate the Entities
	DECLARE @IsError BIT;
	EXEC [bll].[DocumentDefinitions_Validate__Save] 
		@Entities = @Entities,
		@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Save the entities
	EXEC [dal].[DocumentDefinitions__Save]
		@Entities = @Entities,
		@DocumentDefinitionLineDefinitions = @DocumentDefinitionLineDefinitions,
		@ReturnIds = @ReturnIds,
		@UserId = @UserId;
END;
GO

CREATE OR ALTER PROCEDURE [bll].[DocumentDefinitions_Validate__Save]
	@Entities [DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Lookup1DefinitionId',
		N'Error_TheLookupDefinitionForInvoiceTypeTransactionsIsRequired'
	FROM @Entities
	WHERE [ZatcaDocumentType] IN (N'381', N'383', N'388', N'389')
	AND [Lookup1DefinitionId] IS NULL
	UNION
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Lookup2DefinitionId',
		N'Error_TheLookupDefinitionForReasonForIssuanceOfCreditDebitNoteIsRequired'
	FROM @Entities
	WHERE [ZatcaDocumentType] IN (N'381', N'383')
	AND [Lookup2DefinitionId] IS NULL
	UNION
	-- Marmin (UAE): Lookup1 carries profile_execution_id, the supply-scenario flags, but it is
	-- optional: an unset Lookup1 means 00000000, a plain domestic supply, which is the common case.
	--
	-- MarminAeTypeCode is passed to the vendor verbatim, and the vendor accepts exactly these
	-- (docs.ae.marmin.ai/docs/2026-05-07, "Invoice type codes" and "Create a sales credit note"):
	--   SalesInvoice     380  commercial / tax invoice     480  invoice out of scope of tax
	--   SalesCreditNote  381  or 81
	-- Anything else would only be refused by the vendor after a document had closed.
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].MarminAeTypeCode',
		N'Error_MarminAeInvalidTypeCode'
	FROM @Entities
	WHERE ([MarminAeDocumentType] = N'SalesInvoice' AND ISNULL([MarminAeTypeCode], N'') NOT IN (N'380', N'480'))
	OR ([MarminAeDocumentType] = N'SalesCreditNote' AND ISNULL([MarminAeTypeCode], N'') NOT IN (N'381', N'81'))
	UNION
	-- Marmin (UAE): Lookup2 carries discrepancy_response, which the vendor requires on every
	-- credit note.
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Lookup2DefinitionId',
		N'Error_TheLookupDefinitionForReasonForIssuanceOfCreditDebitNoteIsRequired'
	FROM @Entities
	WHERE [MarminAeDocumentType] = N'SalesCreditNote'
	AND [Lookup2DefinitionId] IS NULL
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
GO

CREATE OR ALTER PROCEDURE [dal].[DocumentDefinitions__Save]
	@Entities dbo.[DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE [dbo].[DocumentDefinitions] AS t
		USING @Entities AS s
		ON s.[Id] = t.[Id]
		WHEN MATCHED THEN
			UPDATE SET
				t.[Code]				= s.[Code],
				t.[IsOriginalDocument]	= s.[IsOriginalDocument], 
				t.[Description]			= s.[Description],
				t.[Description2]		= s.[Description2],
				t.[Description3]		= s.[Description3],
				t.[TitleSingular]		= s.[TitleSingular],
				t.[TitleSingular2]		= s.[TitleSingular2],
				t.[TitleSingular3]		= s.[TitleSingular3],
				t.[TitlePlural]			= s.[TitlePlural],
				t.[TitlePlural2]		= s.[TitlePlural2],
				t.[TitlePlural3]		= s.[TitlePlural3],
				t.[Prefix]				= s.[Prefix],
				t.[CodeWidth]			= s.[CodeWidth],

				t.[PostingDateVisibility]= s.[PostingDateVisibility],
				t.[CenterVisibility]	= s.[CenterVisibility],

				t.[Lookup1Label]		= s.[Lookup1Label],
				t.[Lookup1Label2]		= s.[Lookup1Label2],
				t.[Lookup1Label3]		= s.[Lookup1Label3],
				t.[Lookup1Visibility]	= s.[Lookup1Visibility],
				t.[Lookup1DefinitionId]	= s.[Lookup1DefinitionId],
				t.[Lookup2Label]		= s.[Lookup2Label],
				t.[Lookup2Label2]		= s.[Lookup2Label2],
				t.[Lookup2Label3]		= s.[Lookup2Label3],
				t.[Lookup2Visibility]	= s.[Lookup2Visibility],
				t.[Lookup2DefinitionId]	= s.[Lookup2DefinitionId],
				t.[ZatcaDocumentType]	= s.[ZatcaDocumentType],
				t.[MarminAeDocumentType]= s.[MarminAeDocumentType],
				t.[MarminAeTypeCode]	= s.[MarminAeTypeCode],

				t.[ClearanceVisibility]	= s.[ClearanceVisibility],
				t.[MemoVisibility]		= s.[MemoVisibility],

				t.[AttachmentVisibility]= s.[AttachmentVisibility],
				t.[HasBookkeeping]		= s.[HasBookkeeping],
				t.[CloseValidateScript]	= s.[CloseValidateScript],

				t.[MainMenuIcon]		= s.[MainMenuIcon],
				t.[MainMenuSection]		= s.[MainMenuSection],
				t.[MainMenuSortKey]		= s.[MainMenuSortKey],
				t.[SavedById]			= @UserId
		WHEN NOT MATCHED BY TARGET THEN
			INSERT (
				[Code],
				[IsOriginalDocument],
				[Description],
				[Description2],
				[Description3],
				[TitleSingular],
				[TitleSingular2],
				[TitleSingular3],
				[TitlePlural],
				[TitlePlural2],
				[TitlePlural3],
				[Prefix],
				[CodeWidth],
				[PostingDateVisibility],
				[CenterVisibility],
				[Lookup1Label],
				[Lookup1Label2],
				[Lookup1Label3],
				[Lookup1Visibility],
				[Lookup1DefinitionId],
				[Lookup2Label],
				[Lookup2Label2],
				[Lookup2Label3],
				[Lookup2Visibility],
				[Lookup2DefinitionId],
				[ZatcaDocumentType],
				[MarminAeDocumentType],
				[MarminAeTypeCode],
				[ClearanceVisibility],
				[MemoVisibility],
				[AttachmentVisibility],
				[HasBookkeeping],
				[CloseValidateScript],
				[MainMenuIcon],
				[MainMenuSection],
				[MainMenuSortKey],
				[SavedById]
			) VALUES (
				s.[Code],
				s.[IsOriginalDocument],
				s.[Description],
				s.[Description2],
				s.[Description3],
				s.[TitleSingular],
				s.[TitleSingular2],
				s.[TitleSingular3], 
				s.[TitlePlural], 
				s.[TitlePlural2], 
				s.[TitlePlural3],
				s.[Prefix], 
				s.[CodeWidth], 
				s.[PostingDateVisibility], 
				s.[CenterVisibility], 				
				s.[Lookup1Label],
				s.[Lookup1Label2],
				s.[Lookup1Label3],
				s.[Lookup1Visibility],
				s.[Lookup1DefinitionId],
				s.[Lookup2Label],
				s.[Lookup2Label2],
				s.[Lookup2Label3],
				s.[Lookup2Visibility],
				s.[Lookup2DefinitionId],
				s.[ZatcaDocumentType],
				s.[MarminAeDocumentType],
				s.[MarminAeTypeCode],
				s.[ClearanceVisibility], 
				s.[MemoVisibility], 
				s.[AttachmentVisibility], 
				s.[HasBookkeeping], 
				s.[CloseValidateScript],
				s.[MainMenuIcon], 
				s.[MainMenuSection], 
				s.[MainMenuSortKey], 
				@UserId)
		OUTPUT s.[Index], inserted.[Id]
	) AS x;
	
	WITH CurrentDocumentDefinitionLineDefinitions AS (
		SELECT *
		FROM [dbo].[DocumentDefinitionLineDefinitions]
		WHERE [DocumentDefinitionId] IN (SELECT [Id] FROM @Entities)
	)
	MERGE CurrentDocumentDefinitionLineDefinitions AS t
	USING (
		SELECT
			DDLD.[Index],
			DDLD.[Id],
			II.[Id] AS [DocumentDefinitionId],
			DDLD.[LineDefinitionId],
			DDLD.[IsVisibleByDefault]
		FROM @Entities DD
		JOIN @IndexedIds II ON DD.[Index] = II.[Index]
		JOIN @DocumentDefinitionLineDefinitions DDLD ON DD.[Index] = DDLD.[HeaderIndex]
	) AS s
	ON s.Id = t.Id
	WHEN MATCHED THEN
		UPDATE SET
			t.[Index]				= s.[Index],
			t.[LineDefinitionId]	= s.[LineDefinitionId],
			t.[IsVisibleByDefault]	= s.[IsVisibleByDefault],
			t.[SavedById]			= @UserId
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE
	WHEN NOT MATCHED BY TARGET THEN
		INSERT (
			[Index], [DocumentDefinitionId],	[LineDefinitionId], [IsVisibleByDefault], [SavedById]
		) VALUES (
			[Index], s.[DocumentDefinitionId], s.[LineDefinitionId], s.[IsVisibleByDefault], @UserId
		);
	
	-- Signal clients to refresh their cache
	IF EXISTS (SELECT * FROM @IndexedIds I JOIN [dbo].[DocumentDefinitions] D ON I.[Id] = D.[Id] WHERE D.[State] <> N'Hidden')
		UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;
GO

PRINT '--- 3. Views, procedures and validation -------------------------------------';
GO

CREATE OR ALTER FUNCTION [map].[Documents]()
RETURNS TABLE
AS
RETURN (
	SELECT
		D.[Id],
		D.[DefinitionId],
		D.[SerialNumber],
		D.[State],
		D.[StateAt],
		D.[Clearance],
		D.[PostingDate],
		D.[PostingDateIsCommon],
		D.[Memo],
		D.[MemoIsCommon],
		D.[CurrencyId],
		D.[CurrencyIsCommon],
		D.[CenterId],
		D.[CenterIsCommon],
		D.[AgentId],
		D.[AgentIsCommon],
		D.[NotedAgentId],
		D.[NotedAgentIsCommon],
		D.[ResourceId],
		D.[ResourceIsCommon],
		D.[NotedResourceId],
		D.[NotedResourceIsCommon],
		D.[Quantity],
		D.[QuantityIsCommon],
		D.[UnitId],
		D.[UnitIsCommon],
		D.[Time1],
		D.[Time1IsCommon],
		D.[Duration],
		D.[DurationIsCommon],
		D.[DurationUnitId],
		D.[DurationUnitIsCommon],
		D.[Time2],
		D.[Time2IsCommon],
		D.[NotedDate],
		D.[NotedDateIsCommon],

		D.[ExternalReference],
		D.[ExternalReferenceIsCommon],
		D.[ReferenceSourceId],
		D.[ReferenceSourceIsCommon],
		D.[InternalReference],
		D.[InternalReferenceIsCommon],
		D.[Lookup1Id],
		D.[Lookup2Id],
		
		D.[ZatcaState],
		D.[ZatcaResult],
		D.[ZatcaSerialNumber],
		D.[ZatcaHash],
		D.[ZatcaUuid],

		D.[MarminAeState],
		D.[MarminAeDocumentId],
		D.[MarminAeDocumentNumber],
		D.[MarminAeResult],
		D.[MarminAeLastEventId],
		D.[MarminAeLastEventAt],

		D.[CreatedAt],
		D.[CreatedById],
		D.[ModifiedAt],
		D.[ModifiedById],
		[bll].[fn_Prefix_CodeWidth_SN__Code](DD.[Prefix], DD.[CodeWidth], D.[SerialNumber]) AS [Code],
		A.[Comment], A.[AssigneeId], A.[CreatedAt] AS [AssignedAt], A.[CreatedById] AS [AssignedById], A.[OpenedAt]
	FROM [dbo].[Documents] D
	JOIN [dbo].[DocumentDefinitions] DD ON D.[DefinitionId] = DD.[Id]
	LEFT JOIN [dbo].[DocumentAssignments] A ON D.[Id] = A.[DocumentId]
);
GO

CREATE OR ALTER FUNCTION [map].[DocumentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT
		DD.[Id],
		DD.[Code],
		DD.[IsOriginalDocument],
		DD.[Description]	,
		DD.[Description2],
		DD.[Description3],
		DD.[TitleSingular],
		DD.[TitleSingular2]	,
		DD.[TitleSingular3],
		DD.[TitlePlural]	,
		DD.[TitlePlural2]		,
		DD.[TitlePlural3]	,
		DD.[SortKey]	,
		DD.[Prefix]	,
		DD.[CodeWidth]	,
	
		DD.[PostingDateVisibility]	,
		DD.[CenterVisibility]	,
			
		DD.[Lookup1Visibility],
		DD.[Lookup1DefinitionId],
		DD.[Lookup1Label],
		DD.[Lookup1Label2],
		DD.[Lookup1Label3],
	
		DD.[Lookup2Visibility],
		DD.[Lookup2DefinitionId],
		DD.[Lookup2Label],
		DD.[Lookup2Label2],
		DD.[Lookup2Label3],

		DD.[ZatcaDocumentType],
		DD.[MarminAeDocumentType],
		DD.[MarminAeTypeCode],

		DD.[ClearanceVisibility],
		DD.[MemoVisibility],

		DD.[AttachmentVisibility],
		DD.[HasBookkeeping],

		DD.[CloseValidateScript],

		DD.[State],
		DD.[MainMenuIcon],
		DD.[MainMenuSection],
		DD.[MainMenuSortKey],
		DD.[SavedById],
		TODATETIMEOFFSET([ValidFrom], '+00:00') AS [SavedAt],
		DD.[ValidFrom],
		DD.[ValidTo],
		CAST(IIF([1] > 0, 1, 0) AS BIT) AS [CanReachState1],
		CAST(IIF([2] > 0, 1, 0) AS BIT) AS [CanReachState2],
		CAST(IIF([3] > 0, 1, 0) AS BIT) AS [CanReachState3],
		CAST(IIF([1]+[2]+[3]+[4] > 0, 1, 0) AS BIT) AS [HasWorkflow]
	FROM dbo.DocumentDefinitions DD
	LEFT JOIN
	(
		SELECT DDLD.DocumentDefinitionId, W.ToState 
		FROM dbo.DocumentDefinitionLineDefinitions DDLD
		JOIN dbo.Workflows W ON DDLD.LineDefinitionId = W.LineDefinitionId
	) AS ST
	PIVOT  (
		COUNT([ToState]) FOR [ToState] IN ([1], [2], [3], [4])
	) AS PT  ON DD.[Id] = PT.DocumentDefinitionId
);
GO

CREATE OR ALTER PROCEDURE [dal].[MarminAe__GetInvoices]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Given a list of document Ids, maps each one to the information needed to build the
	 * Marmin UAE e-invoice payload (MarminAeSalesInvoiceRequest / MarminAeSalesCreditNoteRequest).
	 *
	 * Modelled on [dal].[Zatca__GetInvoices], but much smaller, because the vendor derives the
	 * supplier party, the document identifiers and EVERY total server-side. The request models
	 * physically cannot express them, so nothing here computes an amount that the vendor will
	 * also compute -- that is exactly the class of mismatch this integration must avoid.
	 *
	 * Unlike ZATCA this is NOT called from [dal].[Documents__Close]. It is a standalone call made
	 * from DocumentsService right after the close, so that a feature used by two tenants adds no
	 * result sets to the close path every tenant runs.
	 *
	 * Constants. These used to be tenant settings and are now fixed, because both tenants use the
	 * same values and a blank setting could only ever produce a vendor rejection:
	 *   endpoint_scheme_id     0235      the UAE Peppol participant scheme
	 *   profile_execution_id   00000000  a plain domestic supply, used when Documents.Lookup1 is unset
	 *   payment_means          30        credit transfer, used when the sales-invoice agent's Lookup1 is unset
	 *   due date               +30 days  after the issue date, used when Documents.NotedDate is unset
	 * bll.Documents_Validate__Close applies the same defaults, so it checks what is actually sent.
	 *
	 * NOTE: the column ordering is important, don't change it. LoadMarminAeInvoices in
	 * SqlDataReaderApplicationExtensions reads these positionally.
	 */

	--=-=-= 0 - Refuse to touch Production from a non-production tenant =-=-=--
	-- Same guard as dal.Zatca__GetInvoices: tenant ids >= 1000 are test/demo databases, and a
	-- restored copy of a production database pointed at the live vendor would transmit real
	-- invoices to real counterparties over Peppol.
	DECLARE @DbName NVARCHAR(50) = DB_NAME();
	DECLARE @DbNameLength INT = LEN(@DbName);
	DECLARE @DotPos INT = CHARINDEX('.', @DbName);
	DECLARE @TenantId INT = CAST(SUBSTRING(@DbName, @DotPos + 1, @DbNameLength - @DotPos) AS INT);
	IF (@TenantId >= 1000) AND (SELECT TOP 1 [MarminAeEnvironment] FROM dbo.Settings) = N'Production'
		THROW 50000, N'Marmin environment cannot be Production in a test tenant. Change it to Sandbox.', 1;

	--=-=-= 1 - Invoice headers =-=-=--
	SELECT
		I.[Index]									AS [Index],
		D.[Id]										AS [Id],

		-- Which vendor endpoint to submit to, and the literal type code the authority expects.
		DD.[MarminAeDocumentType]					AS [DocumentType],
		DD.[MarminAeTypeCode]						AS [TypeCode],

		-- Tellma's own document number becomes the vendor's document_number. Auto-numbering must
		-- be OFF in the Marmin organisation. It is also the key the resubmit path queries the
		-- vendor by, to find out whether a submission that appeared to fail actually landed.
		D.[Code]									AS [DocumentNumber],

		-- The accounting date, NOT StateAt. ZATCA stamps the clearance moment; Peppol wants the
		-- issue date, and it must stay the same if the document is ever resubmitted.
		ISNULL(D.[PostingDate], CAST(D.[StateAt] AS DATE)) AS [IssueDate],

		-- Due date: NotedDate is a free, per-document date with a definition-configurable label,
		-- so tenants relabel it "Due Date". Otherwise 30 days after issue.
		ISNULL(D.[NotedDate], DATEADD(DAY, 30,
			ISNULL(D.[PostingDate], CAST(D.[StateAt] AS DATE)))) AS [DueDate],

		-- The eight supply-scenario flags. Same slot ZATCA uses for InvoiceTypeTransactions, but a
		-- different code vocabulary, so the tenant's Lookup Definition must hold the UAE codes.
		-- With none chosen: a plain domestic supply, or an export (position 8) for a customer
		-- outside the UAE. Peppol accepts a buyer outside the UAE only on an export (ibr-135-ae:
		-- otherwise it needs a tin or TRN, which the vendor forbids on a foreign party), and the
		-- mapper sends the delivery block an export requires.
		ISNULL(dal.fn_Lookup__Code(D.[Lookup1Id]),
			IIF(ISNULL(CUST.[CountryCode], N'AE') = N'AE', N'00000000', N'00000001')) AS [ProfileExecutionId],

		ISNULL(SI.[CurrencyId], D.[CurrencyId])		AS [DocumentCurrencyCode],
		D.[Memo]									AS [Note],
		D.[ExternalReference]						AS [BuyerReference],

		-- Credit notes only: why the note was issued. Required by the vendor on every credit note.
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote',
			dal.fn_Lookup__Code(D.[Lookup2Id]), NULL) AS [DiscrepancyResponse],
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote',
			dal.fn_Lookup__Name(D.[Lookup2Id]), NULL) AS [Reason],

		-- Customer. Latin Name, not Name2: ZATCA uses Name2 because KSA mandates Arabic, UAE
		-- Peppol does not. Falls back from the customer group to the customer account.
		ISNULL(CG.[Name], CA.[Name])				AS [CustomerName],
		ISNULL(CA.[ContactEmail], CG.[ContactEmail]) AS [CustomerEmail],

		-- The Peppol routing address. Whether the customer is reachable on Peppol at all is
		-- recorded on the customer agent's Lookup4, which points at the YesNo lookup definition
		-- (code Y = registered), the same convention bll.ft_Employees__Deductions_SD uses:
		--   registered           -> their 10-digit TIN
		--   not registered, UAE  -> 9900000098, the FTA's placeholder for a UAE buyer not on Peppol
		--   not registered, else -> 9900000099, the FTA's placeholder for an export to a buyer
		--                           outside the UAE
		-- Since 2026-07-07 the vendor no longer defaults to 9900000098 when the endpoint is
		-- omitted, so one of these must always be sent.
		CASE
			WHEN CUST.[IsPeppolRegistered] = 1 THEN TAXID.[Tin]
			WHEN CUST.[CountryCode] = N'AE' THEN N'9900000098'
			ELSE N'9900000099'
		END											AS [CustomerEndpointId],
		N'0235'										AS [CustomerEndpointSchemeId],

		-- tin is the 10-digit TIN and the vendor rejects it on a foreign party. The 15-digit
		-- TRN is a different identifier and travels separately, in party_tax_scheme, which the
		-- vendor forbids for a non-UAE buyer too.
		IIF(CUST.[CountryCode] = N'AE', TAXID.[Tin], NULL) AS [CustomerTin],
		IIF(CUST.[CountryCode] = N'AE', TAXID.[Trn], NULL) AS [CustomerTrn],

		CA.[AddressStreet]							AS [CustomerStreetName],
		CA.[AddressAdditionalStreet]				AS [CustomerAdditionalStreetName],
		CA.[AddressCity]							AS [CustomerCityName],
		CA.[AddressPostalCode]						AS [CustomerPostalZone],
		-- Must be an emirate CODE for a UAE address. bll.Documents_Validate__Close checks it.
		CA.[AddressProvince]						AS [CustomerCountrySubentity],
		dal.fn_Lookup__Name(CA.[AddressCountryId])	AS [CustomerCountry],
		CUST.[CountryCode]							AS [CustomerCountryCode],

		-- Payment. Same slot ZATCA reads, defaulting to 30 (credit transfer), which requires the
		-- payee account (IBR-192-AE); bll.Documents_Validate__Close asserts it is there.
		ISNULL(dal.fn_Lookup__Code(SI.[Lookup1Id]), N'30') AS [PaymentMeansCode],
		SI.[BankAccountNumber]						AS [PayeeFinancialAccountId],

		-- Rounding is modelled as a zero-VAT resource called Rounding; reused from ZATCA as-is.
		dal.fn_Document__RoundingAmount(D.[Id])		AS [PayableRoundingAmount],

		-- Credit notes only: the original invoice this note adjusts. Found by the same NotedAgentId
		-- heuristic ZATCA uses, but written inline: dal.fn_Document__BillingReferenceId declares
		-- RETURNS NVARCHAR with no length, i.e. NVARCHAR(1), so it truncates every code to one
		-- character. bll.Documents_Validate__Close asserts exactly one match before we get here.
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote', OI.[Code], NULL)			AS [BillingReferenceId],
		IIF(DD.[MarminAeDocumentType] = N'SalesCreditNote', OI.[PostingDate], NULL)	AS [BillingReferenceIssueDate],

		-- The vendor's id when it already holds this document. Set only once the vendor has
		-- accepted a submission, so its presence is what decides between a PUT (resubmit the
		-- document it holds, which the vendor allows only while it is VALIDATION_FAILED) and a
		-- POST (create a new one).
		D.[MarminAeDocumentId]						AS [MarminAeDocumentId]
	FROM [map].[Documents]() D
	INNER JOIN @Ids I ON I.[Id] = D.[Id]
	INNER JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	INNER JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]	-- Sales Invoice
	INNER JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id]		-- Customer Account
	LEFT JOIN dbo.Agents CG ON CG.[Id] = CA.[Agent1Id]		-- Customer
	CROSS APPLY (
		SELECT
			dal.fn_Lookup__Code(CA.[AddressCountryId]) AS [CountryCode],
			LTRIM(RTRIM(ISNULL(CA.[TaxIdentificationNumber], CG.[TaxIdentificationNumber]))) AS [TaxId],
			IIF(dal.fn_Lookup__Code(ISNULL(CA.[Lookup4Id], CG.[Lookup4Id])) = N'Y', 1, 0) AS [IsPeppolRegistered]
	) CUST
	CROSS APPLY (
		-- A UAE tax number is held either as the 10-digit TIN (starts with 1) or as the 15-digit
		-- TRN (starts with 1, ends with 03), whose first ten digits are the TIN. Anything else
		-- yields NULL here, and bll.Documents_Validate__Close refuses the close for it.
		SELECT
			CASE
				WHEN LEN(CUST.[TaxId]) = 10 AND CUST.[TaxId] LIKE N'1%' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN CUST.[TaxId]
				WHEN LEN(CUST.[TaxId]) = 15 AND CUST.[TaxId] LIKE N'1%03' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN LEFT(CUST.[TaxId], 10)
			END AS [Tin],
			CASE
				WHEN LEN(CUST.[TaxId]) = 15 AND CUST.[TaxId] LIKE N'1%03' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN CUST.[TaxId]
			END AS [Trn]
	) TAXID
	OUTER APPLY (
		-- An original closed before the definition became Marmin-typed has a NULL MarminAeState
		-- forever, and must still be referencable, since the UAE does not require the original
		-- to have been an e-invoice. NULL is unambiguous: a Marmin-typed close always stamps a
		-- state, so NULL can only mean "closed before go-live" or a clone reset. A rejected
		-- invoice (-30) is included because the vendor's remedy for one is a credit note.
		SELECT TOP 1 O.[Code], O.[PostingDate]
		FROM [map].[Documents]() O
		JOIN dbo.DocumentDefinitions ODD ON ODD.[Id] = O.[DefinitionId]
		WHERE ODD.[MarminAeDocumentType] = N'SalesInvoice'
		AND O.[State] = 1							-- closed
		AND (O.[MarminAeState] IS NULL OR O.[MarminAeState] >= 1 OR O.[MarminAeState] = -30)
		AND O.[NotedAgentId] = D.[NotedAgentId]
		ORDER BY O.[Id] DESC
	) OI
	WHERE DD.[MarminAeDocumentType] IS NOT NULL
	-- Only documents that may be (re)submitted: never claimed (NULL), claimed but never sent (0),
	-- refused by the vendor (-10), or failing Peppol validation (-20, resubmitted with a PUT).
	-- Everything else is on the network (1, 2, 10) or needs a credit note (-30), and the reopen
	-- guard keeps those closed, so a re-close never reaches them.
	AND (D.[MarminAeState] IS NULL OR D.[MarminAeState] IN (0, -10, -20));

	--=-=-= 2 - Invoice lines =-=-=--
	SELECT
		I.[Index]									AS [InvoiceIndex],
		L.[Index] + 1								AS [LineNumber],

		NR.[Name]									AS [Name],
		-- description is required by the vendor but nullable here, so fall back to the name.
		ISNULL(NR.[Description], NR.[Name])			AS [Description],

		-- Sign convention copied from ZATCA's 388 / 381 split.
		IIF(DD.[MarminAeDocumentType] = N'SalesInvoice', -1, +1) * E.[Direction] * E.[Quantity] AS [Quantity],

		-- Must be a UN/ECE Rec 20 code. Free text in Tellma, so validated at close.
		U.[Code]									AS [UnitCode],

		L.[Decimal1]								AS [PriceBaseAmount],
		1.00										AS [PriceBaseQuantity],

		-- UNCL5305 letter, the same vocabulary ZATCA uses.
		ISNULL(LK3.[Code], N'S')					AS [TaxCategoryId],

		-- Marmin wants a PERCENTAGE where ZATCA wants a 0..1 fraction, and the UAE standard rate
		-- is 5%, not KSA's 15%. Resources.VatRate is constrained to 0..1, hence the * 100.
		ISNULL(NR.[VatRate], 0.05) * 100			AS [TaxPercent],

		-- Required by the vendor whenever the category is exempt. ZATCA left these commented out.
		LK4.[Code]									AS [TaxExemptionReasonCode],
		LK4.[Name]									AS [TaxExemptionReason],

		-- standard_item_identification is deliberately NOT sent. Resources.Identifier is free
		-- text (a barcode, a serial, whatever the definition labels it), and PINT-AE IBR-064
		-- rejects an identifier without an ISO 6523 scheme. The vendor's API does not check it,
		-- so the rejection would only surface at Peppol, after the close.
		NR.[Code]									AS [SellerItemIdentification]
	FROM [map].[Lines]() L
	INNER JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	INNER JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
	INNER JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = NR.[DefinitionId]
	LEFT JOIN dbo.Lookups LK3 ON LK3.[Id] = NR.[Lookup3Id]
	LEFT JOIN dbo.Lookups LK4 ON LK4.[Id] = NR.[Lookup4Id]
	INNER JOIN dbo.Units U ON U.[Id] = E.[UnitId]
	INNER JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	INNER JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	INNER JOIN [map].[Documents]() D ON D.[Id] = L.[DocumentId]
	INNER JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	INNER JOIN @Ids AS I ON I.[Id] = D.[Id]
	WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
	AND DD.[MarminAeDocumentType] IS NOT NULL
	AND (D.[MarminAeState] IS NULL OR D.[MarminAeState] IN (0, -10, -20))
	-- A workflow line that was rejected or voided (negative state) is not part of the ledger, so
	-- it must not be part of the invoice either. bll.Documents_Validate__Close lets a document
	-- close with such lines present.
	AND L.[State] >= 0
	-- Discounts, retentions and prepayments are document-level allowances/charges, which are out
	-- of scope for v1.
	AND NOT (NRD.[Code] = N'Discounts' OR NR.[Code] = N'RetentionByCustomer'
		OR NRD.[Code] LIKE N'Prepayments%' AND E.[Direction] = 1)
	ORDER BY I.[Index], L.[Index];
END;
GO

CREATE OR ALTER PROCEDURE [dal].[MarminAe__MarkSubmitting]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Claims a document for submission to Marmin by stamping state 0 (Submitting).
	 *
	 * Called inside the same transaction as the close (or as the Resubmit action's own claim),
	 * before any HTTP call. It accepts exactly the states dal.MarminAe__GetInvoices returns:
	 *   NULL  never submitted
	 *   0     claimed but never sent
	 *   -10   refused by the vendor, so not on the network
	 *   -20   failing Peppol validation, to be resubmitted with a PUT
	 *
	 * MarminAeDocumentId is deliberately left alone: on a -20 document it is the id the PUT
	 * resubmits, and it is NULL in every other accepted state anyway.
	 *
	 * State 0 is below the reopen guard's bar on purpose. Nothing has been sent at this point;
	 * dal.MarminAe__MarkSent moves the document to 2 immediately before the HTTP call, and that
	 * is the state that says "may already be on the network".
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = 0			-- Submitting
	WHERE [Id] = @Id
	AND ([MarminAeState] IS NULL OR [MarminAeState] IN (0, -10, -20));
END;
GO

CREATE OR ALTER PROCEDURE [dal].[MarminAe__MarkSent]
	@Id INT,
	@RowsAffected INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Moves a claimed document from 0 (Submitting) to 2 (SentAwaitingOutcome), in its own
	 * transaction, IMMEDIATELY before the HTTP call to the vendor.
	 *
	 * This is what lets state 0 mean only "claimed, never sent". Once the request leaves, a
	 * timeout or a crash can no longer tell us whether the vendor received it, and the document
	 * must be treated as possibly on the Peppol network: 2 is >= 1, so the reopen guard, the
	 * credit-note original count and the terminal checks all cover it without further change.
	 * "Refresh e-invoice status" and "Resubmit" resolve a document left at 2 by asking the vendor
	 * whether a document with its number exists.
	 *
	 * Conditional on the document still being closed and still at 0, and reported through
	 * @RowsAffected. If either changed since the claim -- reopened in Sandbox, where reopening is
	 * allowed in any state, or picked up by a concurrent resubmit -- the caller must not send.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = 2			-- SentAwaitingOutcome
	WHERE [Id] = @Id
	AND [State] = 1
	AND [MarminAeState] = 0;

	SET @RowsAffected = @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE [dal].[MarminAe__ReleaseUnsent]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Drops a document from 2 (SentAwaitingOutcome) back to 0 (Submitting).
	 *
	 * Called only once the vendor itself has settled that the earlier request did not take
	 * effect, in one of two ways:
	 *   - no vendor id stored: the vendor has no document with this document's number, so the
	 *     POST never landed;
	 *   - vendor id stored (a PUT resubmission that timed out): the vendor still reports the
	 *     document as VALIDATION_FAILED, so it is resubmittable either way.
	 * Back at 0 the document is resubmittable again (dal.MarminAe__GetInvoices accepts 0).
	 *
	 * Conditional on the state still being 2, so a concurrent outcome write that has meanwhile
	 * recorded the vendor's answer is never undone.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = 0			-- Submitting
	WHERE [Id] = @Id
	AND [MarminAeState] = 2;
END;
GO

CREATE OR ALTER PROCEDURE [dal].[MarminAe__UpdateDocumentInfo]
	@Id INT,
	@MarminAeState INT,
	@MarminAeDocumentId NVARCHAR (50) = NULL,
	@MarminAeDocumentNumber NVARCHAR (50) = NULL,
	@MarminAeResult NVARCHAR (MAX) = NULL,
	@MarminAeLastEventAt DATETIMEOFFSET(7) = NULL,
	@RowsAffected INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Records the outcome of a submission, or what the vendor says about a document it already
	 * holds. Mirrors [dal].[Zatca__UpdateDocumentInfo].
	 *
	 * Runs in its own transaction AFTER the close has committed and after the HTTP call has
	 * returned, so a failure here cannot unwind accounting that is already correct. A failure
	 * leaves the document at 2 (SentAwaitingOutcome), which "Refresh e-invoice status" resolves
	 * by asking the vendor for the document's number.
	 *
	 * Conditional on the document still being closed. Reopening is refused from state 1 upward in
	 * Production, but Sandbox allows it in any state and clears the Marmin columns when it does,
	 * so an outcome arriving for a document reopened while the request was in flight would
	 * otherwise stamp "Submitted" and a vendor id onto an open document. @RowsAffected = 0 tells
	 * the caller that happened, so it can say so.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = @MarminAeState,
		[MarminAeDocumentId] = ISNULL(@MarminAeDocumentId, [MarminAeDocumentId]),
		[MarminAeDocumentNumber] = ISNULL(@MarminAeDocumentNumber, [MarminAeDocumentNumber]),
		[MarminAeResult] = @MarminAeResult,
		[MarminAeLastEventAt] = ISNULL(@MarminAeLastEventAt, [MarminAeLastEventAt])
	WHERE [Id] = @Id
	AND [State] = 1;

	SET @RowsAffected = @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE [dal].[MarminAe__ResetOnOpen]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * In Sandbox only, forgets everything Marmin knew about documents that are being reopened.
	 *
	 * bll.Documents_Validate__Open refuses to reopen a submitted document in Production, but
	 * deliberately allows it in Sandbox so that the integration can be exercised repeatedly.
	 * Without this, a reopened and edited sandbox document would keep its vendor id and its
	 * Submitted or Delivered state: a later refresh would report the vendor's verdict on the old
	 * content, the document would still count as a credit-note original, and a re-close would
	 * never submit the new content because dal.MarminAe__GetInvoices skips those states.
	 *
	 * After this a re-close submits afresh. Note that the vendor enforces unique document numbers
	 * within an organisation, so re-sending a number it already holds is refused as a duplicate;
	 * that is the expected sandbox outcome, and the reason the README says to test each scenario
	 * on a fresh document.
	 *
	 * Called from DocumentsService inside the Open transaction, only for Marmin definitions.
	 */

	IF (SELECT TOP 1 [MarminAeEnvironment] FROM [dbo].[Settings]) <> N'Sandbox'
		RETURN;

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = NULL,
		[MarminAeDocumentId] = NULL,
		[MarminAeDocumentNumber] = NULL,
		[MarminAeResult] = NULL,
		[MarminAeLastEventId] = NULL,
		[MarminAeLastEventAt] = NULL
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
	AND [MarminAeState] IS NOT NULL;
END;
GO

CREATE OR ALTER PROCEDURE [dal].[MarminAe__SaveSecrets]
	@EncryptedClientSecret NVARCHAR (MAX) = NULL,
	@EncryptedWebhookSecret NVARCHAR (MAX) = NULL,
	@EncryptionKeyIndex INT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Stores the tenant's Marmin credentials. Mirrors [dal].[Zatca__SaveSecrets].
	 *
	 * Both secrets are already AES-encrypted by the caller. A NULL leaves the stored value
	 * alone, so an administrator can rotate one secret without re-entering the other -- the
	 * screen never sends a secret back to the browser, so it has nothing to re-send.
	 *
	 * CONTRACT: @EncryptionKeyIndex is written unconditionally, and it is the ONE index both
	 * ciphertexts are read back with. So the caller must pass every secret that should survive
	 * the save, encrypted under that one key -- a secret it is not replacing has to be decrypted
	 * with the currently stored index and re-encrypted with the new one, which is what
	 * GeneralSettingsService.SaveMarminAeSecrets does. Passing NULL here means "there is no such
	 * secret", not "keep the old ciphertext under its old key": the latter would leave that
	 * ciphertext undecryptable the moment the index moved.
	 *
	 * Bumping SettingsVersion is what makes every web server drop its cached SettingsForClient
	 * and pick the new credentials up, which is also what invalidates the cached API client.
	 */

	UPDATE [dbo].[Settings]
	SET [MarminAeEncryptedClientSecret] = ISNULL(@EncryptedClientSecret, [MarminAeEncryptedClientSecret]),
		[MarminAeEncryptedWebhookSecret] = ISNULL(@EncryptedWebhookSecret, [MarminAeEncryptedWebhookSecret]),
		[MarminAeEncryptionKeyIndex] = @EncryptionKeyIndex,
		[SettingsVersion] = NEWID();
END;
GO

CREATE OR ALTER PROCEDURE [dal].[MarminAe__ApplyWebhook]
	@MarminAeDocumentId NVARCHAR (50),
	@MarminAeState INT,
	@MarminAeResult NVARCHAR (MAX) = NULL,
	@WebhookEventId UNIQUEIDENTIFIER = NULL,
	@EventTimestamp DATETIMEOFFSET(7) = NULL,
	@RowsAffected INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	/*
	 * Applies an asynchronous status update to the document the vendor identifies.
	 *
	 * Shared by BOTH the webhook and the "Refresh e-invoice status" poll, deliberately: the poll
	 * is how this logic gets exercised until the webhook can be live-tested, so they must not be
	 * two different code paths.
	 *
	 * The WHERE clause is the whole concurrency story, and needs no dedup table:
	 *   - MarminAeLastEventId <> @WebhookEventId drops an exact redelivery. Vendor delivery is
	 *     at-least-once, so the same event WILL arrive twice.
	 *   - MarminAeLastEventAt <= @EventTimestamp drops a stale one. The vendor keeps only the
	 *     latest pending delivery per document, so an old redelivery could otherwise overwrite
	 *     Delivered with Pending.
	 *
	 * Both are the WEBHOOK's story, and both are skipped when the caller passes NULL for them,
	 * which is how the poll identifies itself. A poll has no vendor event id and no vendor
	 * event instant: it has just read the current state, so it is never stale and never a
	 * redelivery. Stamping the app server's own clock into MarminAeLastEventAt to satisfy the
	 * guard would be comparing two different clocks, and would suppress every genuine webhook
	 * whose vendor-side instant happened to precede the poll. So a poll always applies, and
	 * leaves both ordering columns exactly as the last real event left them.
	 *
	 * @RowsAffected lets the caller tell "already applied" from "no such document". Both are
	 * answered with HTTP 200: a 4xx/5xx would make the vendor retry-storm on a document that may
	 * not be ours at all, or that simply has not finished committing yet.
	 */

	UPDATE [dbo].[Documents]
	SET [MarminAeState] = @MarminAeState,
		[MarminAeResult] = @MarminAeResult,
		[MarminAeLastEventId] = ISNULL(@WebhookEventId, [MarminAeLastEventId]),
		[MarminAeLastEventAt] = ISNULL(@EventTimestamp, [MarminAeLastEventAt])
	WHERE [MarminAeDocumentId] = @MarminAeDocumentId
	AND (@WebhookEventId IS NULL OR [MarminAeLastEventId] IS NULL OR [MarminAeLastEventId] <> @WebhookEventId)
	AND (@EventTimestamp IS NULL OR [MarminAeLastEventAt] IS NULL OR [MarminAeLastEventAt] <= @EventTimestamp)
	-- Never move a document backwards out of a verdict. MarminAeService maps an absent or
	-- unrecognised peppol_status to 1 (Submitted), which is the right reading for a document in
	-- flight but would otherwise silently undo a verdict -- and the vendor's status vocabulary is
	-- open, so an unrecognised value is expected rather than exceptional.
	--
	-- 10 (Delivered, APPROVED) and -30 (PeppolRejected, REJECTED) are final: only another final
	-- verdict may replace them.
	AND NOT ([MarminAeState] IN (10, -30) AND @MarminAeState NOT IN (10, -30))
	-- -20 (PeppolValidationFailed) is not final, because the vendor lets it be resubmitted, after
	-- which it may legitimately reach either verdict. But it must not be pushed back to 1 by an
	-- in-flight status: that crosses the >= 1 thresholds bll.Documents_Validate__Open uses to
	-- refuse a reopen, stranding a document whose remedy is precisely to be reopened, fixed and
	-- resubmitted. Our own resubmission records its outcome through
	-- dal.MarminAe__UpdateDocumentInfo, which this guard does not apply to.
	AND NOT ([MarminAeState] = -20 AND @MarminAeState NOT IN (10, -20, -30));

	SET @RowsAffected = @@ROWCOUNT;
END;
GO

CREATE OR ALTER PROCEDURE [bll].[Documents_Validate__Open]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Documents DocumentList, @DocumentLineDefinitionEntries DocumentLineDefinitionEntryList,
			@Lines LineList, @Entries EntryList;

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentWithId0WasNotFound',
		CAST([Id] AS NVARCHAR (255))
    FROM @Ids
    WHERE Id <> 0
	AND Id NOT IN (SELECT Id from [dbo].[Documents]);

	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE

	-- Cannot unpost it if it is not posted
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_1'
	FROM @Ids FE
	JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 1;	

	-- Cannot unpost it if it is a Zatca document
	IF (SELECT [ZatcaEnvironment] FROM dbo.Settings) <> N'Sandbox'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotOpenAZatcaDocument',
		D.[Code]
	FROM @Ids FE
	JOIN map.Documents() D ON FE.[Id] = D.[Id]
	WHERE D.[ZatcaState] = 10	

	-- Cannot unpost it if it is, or may be, on the Marmin (UAE) Peppol network:
	--    1  Submitted             the vendor accepted it; Peppol validation in progress
	--    2  SentAwaitingOutcome   the request left and its outcome is unknown, so it may have landed
	--   10  Delivered
	--  -30  PeppolRejected        delivered and then rejected; the remedy is a credit note
	--
	-- Stricter than the ZATCA rule above, which only refuses once reported. These stay
	-- reopenable, because none of them is on the network and each has a remedy that starts with
	-- fixing the document: NULL (never submitted), 0 (claimed, never sent), -10 (refused by the
	-- vendor) and -20 (failed Peppol validation; a re-close resubmits it).
	--
	-- This single guard also covers delete and cancel: bll.Documents_Validate__Delete refuses a
	-- closed document, and bll.Documents_Validate__Cancel requires State = 0, so both already
	-- require reopening first.
	--
	-- Sandbox is deliberately exempt so the integration can be exercised repeatedly. Instead,
	-- DocumentsService calls dal.MarminAe__ResetOnOpen, which clears the Marmin columns of a
	-- reopened sandbox document so that stale vendor state cannot outlive the content it described.
	IF (SELECT [MarminAeEnvironment] FROM dbo.Settings) <> N'Sandbox'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_CannotOpenAMarminAeDocument',
		D.[Code]
	FROM @Ids FE
	JOIN map.Documents() D ON FE.[Id] = D.[Id]
	WHERE D.[MarminAeState] >= 1 OR D.[MarminAeState] = -30

	-- [C#] cannot open if the document posting date falls in an archived period.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].PostingDate',
		N'Error_FallsinArchivedPeriod'
	FROM @Ids FE
	JOIN dbo.Lines L ON L.[DocumentId] = FE.[Id]
	WHERE L.[PostingDate] <= (SELECT [ArchiveDate] FROM dbo.Settings)
	AND L.[State] > 0;

	-- Actually, we might want to allow opening 
	-- [C#] cannot open if the document posting date falls in a frozen period.
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].PostingDate',
		N'Error_FallsinFrozenPeriod'
	FROM @Ids FE
	JOIN dbo.Lines L ON L.[DocumentId] = FE.[Id]
	WHERE L.[PostingDate] <= (SELECT [FreezeDate] FROM dbo.Settings)
	AND L.[State] > 0;

	INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [ResourceId], [ResourceIsCommon],
		[NotedAgentId], [NotedAgentIsCommon],[NotedResourceId], [NotedResourceIsCommon],
		[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon], 
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]
	)
	SELECT [Id], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],  [ResourceId], [ResourceIsCommon],
		[NotedAgentId], [NotedAgentIsCommon],[NotedResourceId], [NotedResourceIsCommon],
		[CurrencyId], [CurrencyIsCommon], [ExternalReference], [ExternalReferenceIsCommon],
		[ReferenceSourceId], [ReferenceSourceIsCommon], [InternalReference], [InternalReferenceIsCommon]
	FROM dbo.Documents
	WHERE [Id] IN (SELECT [Id] FROM @Ids)

	INSERT INTO @DocumentLineDefinitionEntries(
		[Index], [DocumentIndex], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],
		[NotedAgentId], [NotedAgentIsCommon], [ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon],
		[Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Time2], [Time2IsCommon], [ExternalReference], [ExternalReferenceIsCommon], [InternalReference],
		[InternalReferenceIsCommon])
	SELECT 		[Id], [DocumentId], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon],
		[NotedAgentId], [NotedAgentIsCommon], [ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon],
		[Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Time2], [Time2IsCommon], [ExternalReference], [ExternalReferenceIsCommon], [InternalReference],
		[InternalReferenceIsCommon]
	FROM DocumentLineDefinitionEntries
	WHERE [DocumentId] IN (SELECT [Id] FROM @Ids)
	AND [LineDefinitionId]  IN (SELECT [Id] FROM map.LineDefinitions() WHERE [HasWorkflow] = 0);

	-- Verify that workflow-less lines in Events can be in state draft
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],		[Memo])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo]
	FROM dbo.Lines L
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	AND L.[DefinitionId] IN (SELECT [Id] FROM map.LineDefinitions() WHERE [HasWorkflow] = 0);
	
	INSERT INTO @Entries (
	[Index], [LineIndex], [DocumentIndex], [Id],
	[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId],
	[CenterId],	[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [Time1],
	[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
	[NotedAmount], [NotedDate])
	SELECT
	E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
	E.[Direction],E.[AccountId],E.[CurrencyId],E.[AgentId],E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId],
	E.[CenterId], E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value],E.[Time1],
	E.[Time2],E.[ExternalReference],E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
	E.[NotedAmount],E.[NotedDate]
	FROM dbo.Entries E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, 
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, 
		@Entries = @Entries, 
		@State = 0, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;

DONE:
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
GO

CREATE OR ALTER PROCEDURE [bll].[Documents_Validate__Close]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Documents [dbo].[DocumentList], @DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList],
			@Lines [dbo].[LineList], @Entries [dbo].[EntryList];
	DECLARE @ManualJV INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');
	SET @IsError = 0;
	DECLARE @EndOfLine NVARCHAR(5) = ',' + CHAR(13) + CHAR(10);
	-- Fill vaidation error list like the others
	DECLARE @Err NVARCHAR(MAX)
	SELECT @Err = STRING_AGG(Err, @EndOfLine) 
	FROM (
		SELECT
		N'Tab: ' + LD.titlesingular +
		N', dbo.Line #: '+ CAST (e.[index] + 1 as nvarchar (50)) +
		N', Account: '+ A.[Name] +
		N', Amount: '+ FORMAT(e.MonetaryValue, N'N2') +
		N', Value: '+ FORMAT(e.[Value], N'N2') +
		N'. Resource: '+ R.[Name] + ' is wrong' AS Err
		FROM entries e
		JOIN dbo.Lines l ON L.id = e.Lineid
		JOIN dbo.LineDefinitions LD ON LD.id = L.definitionid
		JOIN dbo.Accounts A ON A.id = e.accountid
		JOIN dbo.AccountTypes ac ON ac.id = A.accounttypeid
		JOIN dbo.Resources r ON R.id = e.resourceid
		LEFT JOIN AccountTypeResourceDefinitions ACRD ON ACRD.AccountTypeId = ac.id and ACRD.ResourceDefinitionId = R.DefinitionId
		WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
		AND ACRD.id IS NULL
		AND L.[State] >= 0

		UNION

		SELECT
		N'Tab: ' + LD.titlesingular +
		N', dbo.Line #: '+ CAST (e.[index] + 1 as nvarchar (50)) +
		N', Account: '+ A.[Name] +
		N', Amount: '+ FORMAT(e.MonetaryValue, N'N2') +
		N', Value: '+ FORMAT(e.[Value], N'N2') +
		N'. Noted Resource: '+ R.[Name] + ' is wrong'
		FROM entries e
		JOIN dbo.Lines l ON L.id = e.Lineid
		JOIN dbo.LineDefinitions LD ON LD.id = L.definitionid
		JOIN dbo.Accounts A ON A.id = e.accountid
		JOIN dbo.AccountTypes ac ON ac.id = A.accounttypeid
		JOIN dbo.Resources r ON R.id = e.NotedResourceId
		LEFT JOIN AccountTypeNotedResourceDefinitions ACRD ON ACRD.AccountTypeId = ac.id and ACRD.NotedResourceDefinitionId = R.DefinitionId
		WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
		AND ACRD.id IS NULL
		AND L.[State] >= 0

		UNION

		SELECT
		N'Tab: ' + LD.titlesingular +
		N', dbo.Line #: '+ CAST (e.[index] + 1 as nvarchar (50)) +
		N', Account: '+ A.[Name] +
		N', Amount: '+ FORMAT(e.MonetaryValue, N'N2') +
		N', Value: '+ FORMAT(e.[Value], N'N2') +
		N'. Agent: '+ AG.[Name] + ' is wrong'
		FROM entries e
		JOIN dbo.Lines l ON L.id = e.Lineid
		JOIN dbo.LineDefinitions LD ON LD.id = L.definitionid
		JOIN dbo.Accounts A ON A.id = e.accountid
		JOIN dbo.AccountTypes ac ON ac.id = A.accounttypeid
		JOIN agents AG ON AG.id = e.agentid
		LEFT JOIN AccountTypeagentDefinitions ACRD ON ACRD.AccountTypeId = ac.id and ACRD.agentDefinitionId = AG.DefinitionId
		WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
		AND ACRD.id IS NULL
		AND L.[State] >= 0

		UNION

		SELECT
		N'Tab: ' + LD.titlesingular +
		N', dbo.Line #: '+ CAST (e.[index] + 1 as nvarchar (50)) +
		N', Account: '+ A.[Name] +
		N', Amount: '+ FORMAT(e.MonetaryValue, N'N2') +
		N', Value: '+ FORMAT(e.[Value], N'N2') +
		N'. Noted Agent: '+ AG.[Name] + ' is wrong'
		FROM entries e
		JOIN dbo.Lines l ON L.id = e.Lineid
		JOIN dbo.LineDefinitions LD ON LD.id = L.definitionid
		JOIN dbo.Accounts A ON A.id = e.accountid
		JOIN dbo.AccountTypes ac ON ac.id = A.accounttypeid
		JOIN agents AG ON AG.id = e.NotedagentId
		LEFT JOIN AccountTypeNotedagentDefinitions ACRD ON ACRD.AccountTypeId = ac.id and ACRD.NotedagentDefinitionId = AG.DefinitionId
		WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
		AND ACRD.id IS NULL
		AND L.[State] >= 0
	) T

	IF @Err IS NOT NULL
		THROW 50000, @Err, 1;

	-- cannot close if the line posting date falls in an archived period. Logic repeated at line level
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_FallsinArchivedPeriod', NULL
	FROM @Ids FE
	JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE L.[PostingDate] <= (SELECT [ArchiveDate] FROM dbo.Settings)
	AND LD.[LineType] >= 100
	--UNION
	--SELECT DISTINCT TOP (@Top)
	--	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
	--	N'Error_FallsinFrozenPeriod', NULL
	--FROM @Ids FE
	--JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	--JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	--JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	--WHERE L.[PostingDate] <= (SELECT [FreezeDate] FROM dbo.Settings)
	--AND LD.[LineType] >= 100
	UNION
	-- Cannot close it if it is not draft
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_0'
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 0
	UNION
	-- Cannot close it if it has no attachments while attachments are required
	--INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentHasNoAttachment', NULL
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	JOIN [dbo].[DocumentDefinitions]  DD ON D.[DefinitionId] = DD.[Id]
	LEFT JOIN [dbo].[Attachments] A ON D.[Id] = A.[DocumentId]
	WHERE DD.[AttachmentVisibility] = N'Required'
	AND A.[Id] IS NULL;

	-- Cannot close a document where there are no lines, or where all lines have negative state
	-- So, we take all documents and remove from them those with positive states
	WITH NonSatisfactoryDocuments AS (
		SELECT [Index]
		FROM @Ids
		EXCEPT (
			SELECT DISTINCT FE.[Index]
			FROM @Ids FE
			JOIN [dbo].[Lines] L ON L.[DocumentId] = FE.[Id]
			JOIN [map].[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
			WHERE
				LD.[HasWorkflow] = 1 AND L.[State]  = LD.[LastLineState]
			OR	LD.[HasWorkflow] = 0
		)
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top) 
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentDoesNotHaveAnyPostedLines'
	FROM @Ids
	WHERE [Index] IN (SELECT [Index] FROM NonSatisfactoryDocuments);

	-- Cannot close a document which has lines with missing signatures
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasLinesWithMissingSignatures'
	FROM @Ids FE
	JOIN [dbo].[Lines] L ON L.[DocumentId] = FE.[Id]
	JOIN [map].[LineDefinitions]() LD ON LD.[Id] = L.[DefinitionId]
	WHERE LD.[HasWorkflow] = 1 AND L.[State] BETWEEN 0 AND LD.[LastLineState] - 1;

	-- To do: cannot close a document with a control account having non zero balance
	IF (@DefinitionId <> @ManualJV)
	AND EXISTS (
		SELECT * FROM
		dbo.DocumentDefinitionLineDefinitions DDLD
		JOIN dbo.LineDefinitions LD ON LD.[Id] = DDLD.[LineDefinitionId]
		WHERE DDLD.[DocumentDefinitionId] = @DefinitionId
		AND LD.[LineType] >= 100 -- N'Event', N'Regulatory'
	)
	WITH ControlAccountTypes AS (
		SELECT [Id]
		FROM [dbo].[AccountTypes]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM [dbo].[AccountTypes] WHERE [Concept] = N'ControlAccountsExtension')
		) = 1
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasControlAccount0For1WithNetBalance2' AS [ErrorName],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) As AccountName,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS NotedAgent,
		FORMAT(SUM(E.[Direction] * E.[MonetaryValue]), 'G', 'en-us') AS NetBalance
	FROM @Ids D
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[LineDefinitions] LD ON LD.[Id] = L.[DefinitionId]
	JOIN [dbo].[Entries] E ON E.[LineId] = L.[Id]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.[Id]
	-- MA: LEFT JOIN => JOIN, assuming control accounts have Noted Agent. 2021.12.11
	JOIN [dbo].[Agents] R ON E.[NotedAgentId] = R.[Id]
	WHERE A.AccountTypeId IN (SELECT [Id] FROM ControlAccountTypes)
	AND LD.[LineType] >= 100 -- N'Event', N'Regulatory'
	AND L.[State] >= 0 -- to cater for both Draft in workflow-less and for posted.
	GROUP BY D.[Index], [dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]), E.[CurrencyId], E.[CenterId], [dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) 
	HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	UNION
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasControlAccount0For1WithNetBalance2' AS [ErrorName],
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) As AccountName,
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS NotedAgent,
		FORMAT(SUM(E.[Direction] * E.[Value]), 'G', 'en-us') AS NetBalance
	FROM @Ids D
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[LineDefinitions] LD ON LD.[Id] = L.[DefinitionId]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	-- MA: LEFT JOIN => JOIN, assuming control accounts have Noted Agent. 2021.12.11
	JOIN [dbo].[Agents] R ON E.[NotedAgentId] = R.[Id]
	WHERE A.AccountTypeId IN (SELECT [Id] FROM ControlAccountTypes)
	AND LD.[LineType] >= 100
	AND L.[State] >= 0 -- to cater for both Draft in workflow-less and for posted.
	-- MA: removed CurrencyId From GROUP BY, 2021.12.11
	GROUP BY D.[Index], [dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]), E.[CenterId], [dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) 
	HAVING SUM(E.[Direction] * E.[Value]) <> 0

	-- cannot close a document with sales invoice, if it violates one of the following
	DECLARE @Country NCHAR (2) = dal.fn_Settings__Country();
	IF @Country = N'SA' AND @DefinitionId <> @ManualJV
	AND EXISTS(
		SELECT *
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN @Ids D ON D.[Id] = L.[DocumentId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
	)

	-- If there are ZATCA documents, assert that all ZATCA rules are observed
	IF [dal].[fn_DocumentDefinition__IsZatcaDocumentType](@DefinitionId) = 1
	BEGIN
		INSERT INTO @ValidationErrors([Key], [ErrorName])
		-- Missing invoice type transaction
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheDocumentHasMissingInvoiceTypeTransaction'
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE D.[Lookup1Id] IS NULL
		UNION
		-- Missing invoice
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheDocumentHasMissingInvoice'
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE D.[NotedAgentId] IS NULL
		UNION
		-- Missing invoice currency
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheInvoiceHasMissingCurrency'
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents NAG ON NAG.[Id] = D.[NotedAgentId]
		WHERE NAG.[CurrencyId] IS NULL
		UNION
		-- Wrong date
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheDocumentPostingDateMustBeToday'
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE D.[PostingDate] <> CAST(GETDATE() AS DATE) 
	END
	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE;

	-- If this is a Marmin (UAE) document definition, assert everything the vendor requires is
	-- present BEFORE the close. The vendor call happens after the close commits, so anything it
	-- would refuse must be refused here instead, while the close can still be rolled back;
	-- otherwise the document is closed, claimed and refused, and has to be fixed and resubmitted.
	--
	-- Every fact below is computed the way dal.MarminAe__GetInvoices computes it, defaults
	-- included, so that what is checked is exactly what would be sent.
	IF (SELECT [MarminAeDocumentType] FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId) IS NOT NULL
	BEGIN
		DECLARE @MarminAeDocumentType NVARCHAR (20), @MarminAeTypeCode NVARCHAR (10);
		SELECT @MarminAeDocumentType = [MarminAeDocumentType], @MarminAeTypeCode = [MarminAeTypeCode]
		FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId;
		DECLARE @MarminAeIsCreditNote BIT = IIF(@MarminAeDocumentType = N'SalesCreditNote', 1, 0);

		-- The customer party is reached as NotedAgent -> Agent1, and dal.MarminAe__GetInvoices
		-- reaches it through INNER JOINs. A NULL at either hop would therefore drop the document
		-- from the payload without a word, so both hops are guarded explicitly, as the ZATCA block
		-- above guards the first.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheDocumentHasMissingInvoice',
			CAST(NULL AS NVARCHAR (255))
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE D.[NotedAgentId] IS NULL
		UNION
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_MarminAeDocumentHasNoCustomer',
			SI.[Name]
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]
		WHERE SI.[Agent1Id] IS NULL;

		-- One row per document that does reach a customer.
		DECLARE @MarminAeDocs TABLE (
			[Index]					INT,
			[Id]					INT,
			[Code]					NVARCHAR (255),
			[NotedAgentId]			INT,
			[CustomerName]			NVARCHAR (255),
			[CustomerEmail]			NVARCHAR (255),
			[TaxId]					NVARCHAR (255),
			[Tin]					NVARCHAR (255),
			[IsPeppolRegistered]	BIT,
			[CountryCode]			NVARCHAR (255),
			[Street]				NVARCHAR (255),
			[City]					NVARCHAR (255),
			[Subentity]				NVARCHAR (255),
			[CurrencyId]			NVARCHAR (255),
			[ProfileExecutionId]	NVARCHAR (255),
			[DiscrepancyResponse]	NVARCHAR (255),
			[PaymentMeansCode]		NVARCHAR (255),
			[SalesInvoiceName]		NVARCHAR (255),
			[PayeeAccount]			NVARCHAR (255)
		);

		INSERT INTO @MarminAeDocs
		SELECT
			FE.[Index],
			D.[Id],
			D.[Code],
			D.[NotedAgentId],
			ISNULL(CG.[Name], CA.[Name]),
			ISNULL(CA.[ContactEmail], CG.[ContactEmail]),
			CUST.[TaxId],
			CASE
				WHEN LEN(CUST.[TaxId]) = 10 AND CUST.[TaxId] LIKE N'1%' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN CUST.[TaxId]
				WHEN LEN(CUST.[TaxId]) = 15 AND CUST.[TaxId] LIKE N'1%03' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN LEFT(CUST.[TaxId], 10)
			END,
			IIF(dal.fn_Lookup__Code(ISNULL(CA.[Lookup4Id], CG.[Lookup4Id])) = N'Y', 1, 0),
			dal.fn_Lookup__Code(CA.[AddressCountryId]),
			CA.[AddressStreet],
			CA.[AddressCity],
			CA.[AddressProvince],
			ISNULL(SI.[CurrencyId], D.[CurrencyId]),
			ISNULL(dal.fn_Lookup__Code(D.[Lookup1Id]),
				IIF(ISNULL(dal.fn_Lookup__Code(CA.[AddressCountryId]), N'AE') = N'AE', N'00000000', N'00000001')),
			dal.fn_Lookup__Code(D.[Lookup2Id]),
			ISNULL(dal.fn_Lookup__Code(SI.[Lookup1Id]), N'30'),
			SI.[Name],
			NULLIF(LTRIM(RTRIM(SI.[BankAccountNumber])), N'')
		FROM @Ids FE
		JOIN [map].[Documents]() D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]
		JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id]
		LEFT JOIN dbo.Agents CG ON CG.[Id] = CA.[Agent1Id]
		CROSS APPLY (
			SELECT NULLIF(LTRIM(RTRIM(ISNULL(CA.[TaxIdentificationNumber], CG.[TaxIdentificationNumber]))), N'') AS [TaxId]
		) CUST;

		-- One row per invoice line, exactly as dal.MarminAe__GetInvoices selects them: VAT-payable
		-- entries, not discounts/retentions/prepayment applications, and not on a rejected or
		-- voided workflow line, which is not part of the ledger.
		DECLARE @MarminAeLines TABLE (
			[Index]					INT,
			[DocumentId]			INT,
			[ResourceName]			NVARCHAR (255),
			[Quantity]				DECIMAL (19, 6),
			[Price]					DECIMAL (19, 6),
			[UnitCode]				NVARCHAR (255),
			[TaxCategory]			NVARCHAR (255),
			[HasExemptionReason]	BIT,
			[Vat]					DECIMAL (19, 6)
		);

		INSERT INTO @MarminAeLines
		SELECT
			FE.[Index],
			L.[DocumentId],
			NR.[Name],
			IIF(@MarminAeIsCreditNote = 1, +1, -1) * E.[Direction] * E.[Quantity],
			L.[Decimal1],
			U.[Code],
			ISNULL(LK3.[Code], N'S'),
			IIF(NR.[Lookup4Id] IS NULL, 0, 1),
			ROUND((IIF(@MarminAeIsCreditNote = 1, +1, -1) * E.[Direction] * E.[Quantity])
				* L.[Decimal1] * ISNULL(NR.[VatRate], 0.05), 2)
		FROM @Ids FE
		JOIN [map].[Lines]() L ON L.[DocumentId] = FE.[Id]
		JOIN dbo.Entries E ON E.[LineId] = L.[Id]
		JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
		JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = NR.[DefinitionId]
		LEFT JOIN dbo.Lookups LK3 ON LK3.[Id] = NR.[Lookup3Id]
		LEFT JOIN dbo.Units U ON U.[Id] = E.[UnitId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
		AND L.[State] >= 0
		AND NOT (NRD.[Code] = N'Discounts' OR NR.[Code] = N'RetentionByCustomer'
			OR NRD.[Code] LIKE N'Prepayments%' AND E.[Direction] = 1);

		--=-=-= Customer =-=-=--

		-- The vendor requires an email on the customer party, and Tellma allows it to be blank.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeCustomerHasNoEmail', [CustomerName]
		FROM @MarminAeDocs WHERE [CustomerEmail] IS NULL;

		-- A UAE customer must carry a tax number (it becomes tin and party_tax_scheme), and so must
		-- a Peppol-registered customer anywhere (it becomes the Peppol endpoint). Either way it must
		-- be the 10-digit TIN or the 15-digit TRN, both starting with 1, the TRN ending in 03.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeCustomerHasNoTaxId', [CustomerName]
		FROM @MarminAeDocs WHERE ([CountryCode] = N'AE' OR [IsPeppolRegistered] = 1) AND [TaxId] IS NULL;

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidTaxId', [CustomerName], [TaxId]
		FROM @MarminAeDocs WHERE ([CountryCode] = N'AE' OR [IsPeppolRegistered] = 1) AND [TaxId] IS NOT NULL AND [Tin] IS NULL;

		-- The vendor requires street, city, subentity, country and country code on every address,
		-- UAE or not; the country name is derived from the country, so the country is what is checked.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeCustomerAddressIncomplete', [CustomerName]
		FROM @MarminAeDocs
		WHERE ISNULL([Street], N'') = N'' OR ISNULL([City], N'') = N'' OR ISNULL([Subentity], N'') = N''
		OR ISNULL([CountryCode], N'') = N'';

		-- For a UAE address the subentity must be an emirate CODE. The vendor's own Schematron
		-- asserts exactly this set, and it is what GET /api/codelist/uae-subdivisions returns.
		-- They are three-letter codes (DXB, AUH, ...), not the two-letter ISO 3166-2:AE subdivisions.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidEmirateCode', ISNULL([Subentity], N'')
		FROM @MarminAeDocs
		WHERE [CountryCode] = N'AE'
		AND ISNULL([Subentity], N'') <> N''
		AND [Subentity] NOT IN (N'AUH', N'DXB', N'SHJ', N'UAQ', N'FUJ', N'AJM', N'RAK');

		--=-=-= Document =-=-=--

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_TheInvoiceHasMissingCurrency', CAST(NULL AS NVARCHAR (255))
		FROM @MarminAeDocs WHERE [CurrencyId] IS NULL;

		-- MarminAeTypeCode is also checked when the definition is saved, but a definition saved
		-- before that rule existed would otherwise only be refused by the vendor.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidTypeCode', CAST(NULL AS NVARCHAR (255))
		FROM @MarminAeDocs
		WHERE (@MarminAeDocumentType = N'SalesInvoice' AND ISNULL(@MarminAeTypeCode, N'') NOT IN (N'380', N'480'))
		OR (@MarminAeDocumentType = N'SalesCreditNote' AND ISNULL(@MarminAeTypeCode, N'') NOT IN (N'381', N'81'));

		-- profile_execution_id is validated by the vendor as ^[0-1]{8}$. The defaults are valid, so
		-- this only fires on a Documents.Lookup1 whose code is not eight binary flags.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidProfileExecutionId', [ProfileExecutionId]
		FROM @MarminAeDocs
		WHERE LEN([ProfileExecutionId]) <> 8 OR [ProfileExecutionId] LIKE N'%[^01]%';

		-- A customer outside the UAE can only be invoiced as an export (position 8). Peppol requires
		-- a tin or TRN for any other buyer at a placeholder endpoint (ibr-135-ae), and the vendor
		-- forbids both on a foreign party. Conversely an export is delivered to the customer's
		-- address, which the vendor requires to be outside the UAE.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeForeignCustomerRequiresExport', [CustomerName]
		FROM @MarminAeDocs
		WHERE ISNULL([CountryCode], N'AE') <> N'AE'
		AND LEN([ProfileExecutionId]) = 8 AND SUBSTRING([ProfileExecutionId], 8, 1) <> N'1';

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeExportRequiresForeignCustomer', [CustomerName]
		FROM @MarminAeDocs
		WHERE [CountryCode] = N'AE'
		AND LEN([ProfileExecutionId]) = 8 AND SUBSTRING([ProfileExecutionId], 8, 1) = N'1';

		-- The vendor requires the buyer's legal registration (type and number) on every
		-- out-of-scope invoice, and Tellma does not model one, so a 480 would always be refused
		-- after the close. The 480 rules below still run, so every problem is reported at once.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAe480NotSupported', [Code]
		FROM @MarminAeDocs
		WHERE @MarminAeTypeCode = N'480';

		-- An out-of-scope invoice (480) cannot be a deemed supply (position 2), a profit margin
		-- scheme supply (position 3) or a summary invoice (position 4) -- Peppol IBR-157-AE.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAe480IncompatibleScenario', [ProfileExecutionId]
		FROM @MarminAeDocs
		WHERE @MarminAeTypeCode = N'480'
		AND (SUBSTRING([ProfileExecutionId], 2, 1) = N'1' OR SUBSTRING([ProfileExecutionId], 3, 1) = N'1'
			OR SUBSTRING([ProfileExecutionId], 4, 1) = N'1');

		-- The payment means code (from the sales-invoice agent's Lookup1, default 30) must be one
		-- the vendor accepts (GET /api/codelist/payment-means-modes). ZATCA's 42 and 48 are not.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidPaymentMeans', [PaymentMeansCode]
		FROM @MarminAeDocs
		WHERE [PaymentMeansCode] NOT IN (N'1', N'10', N'20', N'21', N'30', N'49', N'54', N'55', N'68');

		-- A credit transfer must say where to pay (IBR-192-AE): the sales invoice's bank account.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeMissingPayeeAccount', [SalesInvoiceName]
		FROM @MarminAeDocs
		WHERE [PaymentMeansCode] = N'30' AND [PayeeAccount] IS NULL;

		-- A credit note must give its reason as one of the UAE FTA DL8.61.1 codes
		-- (GET /api/codelist/credit-note-reason-codes), held in Documents.Lookup2.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidCreditNoteReason', ISNULL([DiscrepancyResponse], N'')
		FROM @MarminAeDocs
		WHERE @MarminAeIsCreditNote = 1
		AND ISNULL([DiscrepancyResponse], N'') NOT IN (N'DL8.61.1.A', N'DL8.61.1.B', N'DL8.61.1.C', N'DL8.61.1.D', N'DL8.61.1.E', N'VD');

		-- A credit note must name exactly one original invoice. Zero leaves billing_reference
		-- empty, which the vendor rejects; more than one means the NotedAgentId heuristic cannot
		-- tell which invoice is being adjusted. An original closed before the definition became
		-- Marmin-typed (NULL state) counts: the UAE does not require it to have been an e-invoice,
		-- and NULL can only mean that, since a Marmin-typed close always stamps a state. A rejected
		-- invoice (-30) counts too, because the vendor's remedy for one is a credit note. Invoices
		-- refused by the vendor (-10), failing validation (-20) or never sent (0) are not on the
		-- network, and are fixed and resubmitted rather than credited.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST(MD.[Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeNoOriginalInvoice', MD.[Code]
		FROM @MarminAeDocs MD
		WHERE @MarminAeIsCreditNote = 1
		AND (
			SELECT COUNT(*)
			FROM dbo.Documents O
			JOIN dbo.DocumentDefinitions ODD ON ODD.[Id] = O.[DefinitionId]
			WHERE ODD.[MarminAeDocumentType] = N'SalesInvoice'
			AND O.[State] = 1
			AND (O.[MarminAeState] IS NULL OR O.[MarminAeState] >= 1 OR O.[MarminAeState] = -30)
			AND O.[NotedAgentId] = MD.[NotedAgentId]
		) <> 1;

		--=-=-= Lines =-=-=--

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST(MD.[Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeNoInvoiceLines', MD.[Code]
		FROM @MarminAeDocs MD
		WHERE NOT EXISTS (SELECT * FROM @MarminAeLines ML WHERE ML.[DocumentId] = MD.[Id]);

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidQuantity', [ResourceName]
		FROM @MarminAeLines WHERE ISNULL([Quantity], 0) <= 0;

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeMissingPrice', [ResourceName]
		FROM @MarminAeLines WHERE [Price] IS NULL;

		-- unit_code must be a UN/ECE Recommendation 20 code. It is free text in Tellma, so the most
		-- we can assert here is that there is one at all.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidUnitCode', [ResourceName]
		FROM @MarminAeLines WHERE ISNULL([UnitCode], N'') = N'';

		-- The vendor requires an exemption reason on every exempt line (Resources.Lookup4).
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeMissingExemptionReason', [ResourceName]
		FROM @MarminAeLines WHERE [TaxCategory] = N'E' AND [HasExemptionReason] = 0;

		-- A tax invoice (380) cannot consist only of exempt (E) or out-of-scope (O) lines. The
		-- vendor refuses it ("Commercial invoice cannot contain only exempt or out of scope
		-- lines"); a zero-rated (Z) line is enough, which is what makes a zero-rated export a 380.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST(MD.[Index] AS NVARCHAR (255)) + ']', N'Error_MarminAe380RequiresTaxableLine', MD.[Code]
		FROM @MarminAeDocs MD
		WHERE @MarminAeTypeCode = N'380'
		AND EXISTS (SELECT * FROM @MarminAeLines ML WHERE ML.[DocumentId] = MD.[Id])
		AND NOT EXISTS (SELECT * FROM @MarminAeLines ML WHERE ML.[DocumentId] = MD.[Id] AND ML.[TaxCategory] NOT IN (N'E', N'O'));

		-- An out-of-scope invoice (480) may only have E, Z or O lines -- Peppol IBR-122-AE.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAe480RequiresOutOfScopeLines', [ResourceName]
		FROM @MarminAeLines
		WHERE @MarminAeTypeCode = N'480'
		AND [TaxCategory] NOT IN (N'E', N'Z', N'O');

		-- The reconciliation check, and the highest-value assertion here.
		--
		-- Marmin computes every total server-side from the lines we send, so a line-mapping error
		-- does not fail loudly: it produces a legally transmitted invoice whose total differs from
		-- the ledger, discovered later by the customer's counterparty. Recomputing the VAT exactly
		-- the way dal.MarminAe__GetInvoices will emit it, and comparing against what the ledger
		-- says, catches that here -- while the close can still be refused.
		--
		-- The ledger side is the logic of dal.fn_Document__InvoiceTotalVatAmountInAccountingCurrency
		-- inlined, with one addition: rejected and voided workflow lines are excluded, as they are
		-- from the payload. The shared function is left alone because ZATCA's BT-111 uses it.
		--
		-- The tolerance absorbs per-line rounding only; a real mapping error (an unmodelled
		-- discount, a missed line) is far larger than a cent.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST(MD.[Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeTotalsMismatch', MD.[Code]
		FROM @MarminAeDocs MD
		CROSS APPLY (
			SELECT SUM(ML.[Vat]) AS [Vat] FROM @MarminAeLines ML WHERE ML.[DocumentId] = MD.[Id]
		) MAPPED
		CROSS APPLY (
			SELECT ISNULL(-SUM(E.[Direction] * E.[Value]), 0) AS [Vat]
			FROM dbo.Entries E
			JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
			JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
			JOIN dbo.Lines L ON L.[Id] = E.[LineId]
			JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
			WHERE L.[DocumentId] = MD.[Id]
			AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
			AND NR.[Code] NOT LIKE N'Prepayment%'
			AND L.[State] >= 0
		) LEDGER
		WHERE ABS(ISNULL(MAPPED.[Vat], 0) - ABS(LEDGER.[Vat])) > 0.02;
	END
	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE;
	-- Verify that workflow-less lines in Documents can be in their final state
	INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]	
	)
	SELECT Ids.[Index], D.[Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	FROM [dbo].[Documents] D JOIN @Ids Ids ON D.[Id] = Ids.[Id]

	INSERT INTO @DocumentLineDefinitionEntries(
		[Index], [DocumentIndex], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	)
	SELECT 	DLDE.[Id], Ids.[Index], DLDE.[Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	FROM DocumentLineDefinitionEntries DLDE
	JOIN @Ids Ids ON DLDE.[DocumentId] = Ids.[Id]
	AND [LineDefinitionId]  IN (SELECT [Id] FROM [map].[LineDefinitions]() WHERE [HasWorkflow] = 0);

	-- Verify that lines whose last state = approved meet the conditions to be approved
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
			[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
			L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
	FROM [dbo].[Lines] L
	JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]
	WHERE LD.[LastLineState] = 2
	
	INSERT INTO @Entries (
		[Index], [LineIndex], [DocumentIndex], [Id],
		[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
		[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
		[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
		[NotedAmount], [NotedDate])
	SELECT
		E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
		E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
		E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
		E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
		E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	IF EXISTS(SELECT * FROM @Lines)
--	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__Transition_ToState]
		@Documents = @Documents, 
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @ToState = 2, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 RETURN; -- to avoid NESTED INSERT EXEC

	IF EXISTS(SELECT * FROM @Lines)
	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, @DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @State = 2,
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 GOTO DONE;

	DELETE FROM @Lines; DELETE FROM @Entries;
	-- Verify that lines whose last state = posted meet the conditions to be posted
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
			[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
			L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
	FROM [dbo].[Lines] L
	JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
	JOIN @Ids FE ON FE.[Id] = L.[DocumentId]
	JOIN [map].[Documents]() D ON D.[Id] = FE.[Id]
	WHERE LD.[LastLineState] = 4

	INSERT INTO @Entries (
		[Index], [LineIndex], [DocumentIndex], [Id],
		[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
		[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
		[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
		[NotedAmount], [NotedDate])
	SELECT
		E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
		E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
		E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
		E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
		E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	IF EXISTS(SELECT * FROM @Lines)
--	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__Transition_ToState]
		@Documents = @Documents, 
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @ToState = 4, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 RETURN; -- to avoid NESTED INSERT EXEC

	IF EXISTS(SELECT * FROM @Lines)
	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, @DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @State = 4,
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 GOTO DONE;

	DECLARE @CloseValidateScript NVARCHAR (MAX) = (SELECT [CloseValidateScript] FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId);
	IF @CloseValidateScript IS NOT NULL
	BEGIN TRY
		DELETE FROM @Lines; DELETE FROM @Entries;
		-- Pass @Lines and @Entries to the vlidate script
		INSERT INTO @Lines(
				[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
				[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
		SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
				L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
		FROM [dbo].[Lines] L
		JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
		JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
		JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]

		INSERT INTO @Entries (
			[Index], [LineIndex], [DocumentIndex], [Id],
			[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
			[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
			[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
			[NotedAmount], [NotedDate])
		SELECT
			E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
			E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
			E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
			E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
			E.[NotedAmount],E.[NotedDate]
		FROM [dbo].[Entries] E
		JOIN @Lines L ON E.[LineId] = L.[Id];

		INSERT INTO @ValidationErrors
		EXECUTE	dbo.sp_executesql @CloseValidateScript, N'
			@DefinitionId INT,
			@Documents [dbo].[DocumentList] READONLY,
			@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
			@Lines [dbo].[LineList] READONLY, 
			@Entries [dbo].EntryList READONLY,
			@Top INT,
			@UserId INT', 	@DefinitionId = @DefinitionId, @Documents = @Documents,
			@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries, @Lines = @Lines, @Entries = @Entries, @Top = @Top, @UserId = @UserId;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorNumber INT = 100000 + ERROR_NUMBER();
		DECLARE @ErrorMessage NVARCHAR (255) = ERROR_MESSAGE();
		DECLARE @ErrorState TINYINT = 99;
		THROW @ErrorNumber, @ErrorMessage, @ErrorState;
	END CATCH
DONE:
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;
	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
GO

PRINT '--- 4. Verification ---------------------------------------------------------';
GO

-- Expected: 14 Columns (4 Settings, 6 Documents, 2 DocumentDefinitions, and the same 2 again on
-- DocumentDefinitionsHistory, which SQL Server adds itself because DocumentDefinitions is a
-- system-versioned temporal table), 1 Constraint, 2 TypeColumns, 1 Index, and 15 Objects
-- (8 MarminAe procedures, 2 map functions, 5 changed procedures).
SELECT 'Column' AS [Kind], OBJECT_NAME(c.[object_id]) AS [Parent], c.[name] AS [Name]
FROM sys.columns c
JOIN sys.tables t ON t.[object_id] = c.[object_id]   -- real tables only: sys.columns also
WHERE c.[name] LIKE 'MarminAe%'                      -- carries table types and function results
UNION ALL
SELECT 'Constraint', OBJECT_NAME([parent_object_id]), [name]
FROM sys.check_constraints
WHERE [name] = 'CK_Settings__MarminAeEnvironment'
UNION ALL
SELECT 'TypeColumn', 'DocumentDefinitionList', c.[name]
FROM sys.table_types tt
JOIN sys.columns c ON c.[object_id] = tt.[type_table_object_id]
WHERE tt.[name] = 'DocumentDefinitionList' AND c.[name] LIKE 'MarminAe%'
UNION ALL
SELECT 'Index', 'Documents', [name]
FROM sys.indexes
WHERE [name] = 'IX_Documents__MarminAeDocumentId'
UNION ALL
-- The parentheses matter: AND binds tighter than OR, so without them the type filter would
-- apply only to the second half and the MarminAe procedures would be reported unfiltered.
-- map.Documents and map.DocumentDefinitions are inline table-valued functions ('IF'), not views.
SELECT 'Object', SCHEMA_NAME([schema_id]), [name]
FROM sys.objects
WHERE ([name] LIKE 'MarminAe%'
       OR [name] IN ('Documents', 'DocumentDefinitions', 'DocumentDefinitions__Save',
                     'DocumentDefinitions_Validate__Save', 'Documents_Validate__Open',
                     'Documents_Validate__Close'))
  AND [type] IN ('P', 'V', 'IF', 'TF')
ORDER BY [Kind], [Parent], [Name];
GO

DECLARE @CashPurchases dbo.DocumentList, @CashPurchasesLines dbo.DocumentLineList, @CashPurchasesLineEntries dbo.DocumentLineEntryList;

DECLARE @OldCashPurchasesWideLines AS TABLE (
		[Index]						INT PRIMARY KEY,
		[LineDefinitionId]			NVARCHAR (255),
		[DocumentIndex]				INT,
		[EntryNumber]				INT,
		[Direction]					SMALLINT,
		[AccountId]					INT,
		-- Account search parameters. Filled by user upon save, and by B# upon select.
		[Code]						NVARCHAR (50), -- used for import.
		[PartyReference]			NVARCHAR (50), -- how it is referred to by the other party
		[ResponsibilityCenterId]	INT,
		[CustodianId]				INT,
		[ResourceId]				INT,
		[LocationId]				INT,
		--
		[EntryTypeId]				NVARCHAR (255),
		[Memo]						NVARCHAR (255),
		[BatchCode]					NVARCHAR (50),
		[DueDate]					DATE, -- applies to temporary accounts, such as loans and borrowings
		[ExternalReference]			NVARCHAR (255),
		[AdditionalReference]		NVARCHAR (255),
		[RelatedResourceId]			INT, -- Good, Service, Labor, Machine usage
		[RelatedAgentId]			INT,
		[RelatedQuantity]			MONEY,		-- used in Tax accounts, to store the quantiy of taxable item
		[RelatedMonetaryAmount]		MONEY, -- e.g., amount subject to tax
		[Time1]						TIME (0),	-- from time
		[Time2]						TIME (0),	-- to time
	-- Tracking additive measures, the data type is to be decided by AA
		[Area]						DECIMAL (18,2)	NOT NULL DEFAULT 0,
		[Count]						DECIMAL (18,2)	NOT NULL DEFAULT 0,
		[Length]					DECIMAL (18,2)	NOT NULL DEFAULT 0,
		[Mass]						DECIMAL (18,2)	NOT NULL DEFAULT 0,
		[MonetaryValue]				MONEY			NOT NULL DEFAULT 0,
		[Time]						DECIMAL (18,2)	NOT NULL DEFAULT 0, -- ServiceTimeUnit
		[Volume]					DECIMAL (18,2)	NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping

		[Value]						VTYPE			NOT NULL DEFAULT 0
	);
/*
Two main use cases: Accountant and normal user
- Accountant: would use the screen after receiving supporting documents.
- User: would confirm that 
	a) he/she gave the supplier the money, and would attach the evidence
	b) he/she received the fixed asset, and would sign
If the user has stable Internet access, and can deal with the computer comfortably, it is better to ask him/her to enter the info, since it reduces
the amount of data copying. Also, it uses digital signature for authentication.
If user does not have access (either physically or mentally), he fills a properly designed paper form, and the cycle proceeds.
If the accountant is entering several events together (payment and receipt), he might as well combine them.
Hence, cash purchase is valid for:
a) an accountant doing the bookkeeping after receiving all supporting documents
b) a petty cash owner
When the user does not have stable Internet access, the ERP module would be acting simply as data validation and persistence.
When the user has stable Internet access, the ERP module would be responsible for orchestration and business logic.
When an accountant is doing the bookkeeping, she should be able to specify the account directly
When a user is doing the bookkeeping of petty cash, the system should be:
a) powerful enough to capture information such as the beneficiary, etc.
b) flexible enough to allow entering amount, beneficiary and description and let the accountant convert the line to proper accounting entry
1	Dr. PPE
2	Dr. VAT
3		Cr. Payable
*/
DECLARE @CashPurchasesWideLines AS TABLE (
		[Index]						INT PRIMARY KEY,
		[LineDefinitionId]			NVARCHAR (255),
		[DocumentIndex]				INT,
		-- first entry
		[Direction]					SMALLINT, -- +1 from definition
		[AccountId]					INT,	-- filled from search params
		-- Account search parameters. Filled by user upon save, and by B# upon select.
		[Code]						NVARCHAR (50), -- used for import.
		[PartyReference]			NVARCHAR (50), -- how it is referred to by the other party	
		[ResourceId]				INT, -- Data Entry, depending on resource definition, it is either custodian or location
		[CustodianId]				INT, -- Data Entry / depends on resource definition
		[LocationId]				INT, -- Data Entry / depends on resource definition
		[ResponsibilityCenterId]	INT, -- Data Entry
		--
		[EntryTypeId]				NVARCHAR (255), -- Acquisition.. from definition
		[Memo]						NVARCHAR (255), 
		[BatchCode]					NVARCHAR (50),
		[DueDate]					DATE, -- applies to temporary accounts, such as loans and borrowings
		[ExternalReference]			NVARCHAR (255), -- Invoice Number, as from account definition
		[AdditionalReference]		NVARCHAR (255),
		[RelatedResourceId]			INT, -- Good, Service, Labor, Machine usage	
		[RelatedAgentId]			INT, -- supplier as from account definition
		[RelatedQuantity]			MONEY,	-- used in Tax accounts, to store the quantiy of taxable item
		[RelatedMonetaryAmount]		MONEY, -- Value before tax, as from account definition
	-- Depending on the resource definition, the applicable measures will become visible. Since we are dealing with PPE, the PPE record
	-- will have a question about the unit with which the usage will be measured, such as time, kms, or count.
	-- Note that the invoice may show Lenovo X30, 100 pcs.
	-- what about selling chicken by count and weight? if we use Measure and Measure2, 
	-- the weight might be fixed like steel bars, or vary like chicken.
		[Area]						DECIMAL (18,2)	NOT NULL DEFAULT 0, -- depends on resource definition
		[Count]						DECIMAL (18,2)	NOT NULL DEFAULT 0, -- 
		[Length]					DECIMAL (18,2)	NOT NULL DEFAULT 0,
		[Mass]						DECIMAL (18,2)	NOT NULL DEFAULT 0,
		[MonetaryValue]				MONEY			NOT NULL DEFAULT 0,
		[Time]						DECIMAL (18,2)	NOT NULL DEFAULT 0, -- ServiceTimeUnit
		[Volume]					DECIMAL (18,2)	NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping

		[Value]						VTYPE			NOT NULL DEFAULT 0
	);
BEGIN
	INSERT INTO @CashPurchases
	([Index],	[DocumentDate], [SupplierAccountId], [Memo]) VALUES
	(0,			'2018.02.01',	N'Projector for Exec office'), -- fixed asset
	(1,			'2018.02.03',	N'Fuel for machinery'), -- inventory
	(2,			'2018.02.05',	N'HP laser jet ink + SQL Server 2019 License'); -- Consumables + Intangible
	
	

	INSERT INTO @CashPurchasesLines
	([Index], [DocumentIndex], [LineDefinitionId]) VALUES
	(0,			0,				N'PropertyPlandAndEquipmentReceiptWithInvoice'), -- {PPE:resource, lifetime, price excluding VAT}, {VAT , Invoice #, Supplier}
	(1,			0,				N'BalancesWithBanksPaymentIssue'), -- Bank, Branch, check #, Payment
	(2,			1,				N'InventoryReceiptWithInvoice'),
	(3,			1,				N'CashOnHandPaymentIssue'),
	(4,			2,				N'ConsumablesReceiptWithInvoice'),
	(5,			2,				N'IntangibleAssetsReceiptWithInvoice'),
	(6,			2,				N'CreditCardPaymentIssue');
	-- resource definition list is specified by LineDefinition
	-- if user has right to add resources, then user selects a definition, then adds the resource details
	-- otherwise, user can only select what is available
	DECLARE @NewPPEList dbo.ResourceList;
	INSERT INTO @NewPPEList([Name], [ResourceClassificationId])
	VALUES(N'Epson T330 Projector', NULL);
	EXEC api.Resources__Save @DefinitionId = N'property-plant-and-equipment', @Entities = @NewPPEList, @ReturnIds = 1;
	DECLARE @NewPPE INT = (SELECT [Id] FROM dbo.Resources WHERE ResourceDefinitionId = N'property-plant-and-equipment' AND [Name] = N'Epson T330 Projector');
	-- Resources created but not used will be garbage collected.

	-- AccountTypeList specified by LineDefinition
	DECLARE @A00 INT;
	WITH PPEAccountTypes 
	AS (
		SELECT [Id] FROM dbo.AccountTypes
		WHERE [Node].IsDescendantOf(
				-- To allow system account creation, we cannot have a list of account types, 
			(SELECT [Node] FROM dbo.AccountTypes WHERE [Id] = N'PropertyPlantAndEquipment') 
		) = 1
	)
	SELECT @A00 = [Id] FROM dbo.Accounts
	WHERE [ResourceId] = @NewPPE
	AND [AccountTypeId] IN (SELECT [Id] FROM PPEAccountTypes);
	IF @A00 IS NULL
	BEGIN
		-- Possibility of Auto account creation is specified by LineDefinition
		DECLARE @PPEAccounts dbo.AccountList;
		INSERT INTO @PPEAccounts([Index],
		[AccountTypeId],						[AccountClassificationId],	[Name],	[Code])
		SELECT 0,N'PropertyPlantAndEquipment',	NULL,						[Name],	NULL -- or can be later auto-computed
		FROM dbo.Resources WHERE [Id] = @NewPPE;
		-- Accountant can change account type (important) to sub-type and classification (if classification matters)
		-- we may need to store a value called systemType which restricts editing the type to a subtype only
		EXEC [api].[Accounts__Save] --  N'cash-and-cash-equivalents',
			-- Account definition specified by LineDefinition
			@DefinitionId = N'property-plant-and-equipment-accounts',
			@Entities = @PPEAccounts,
			@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

		IF @ValidationErrorsJson IS NOT NULL 
		BEGIN
			Print 'Inserting PPE Accounts'
			GOTO Err_Label;
		END;
		
		SELECT @A00 = [Id] FROM dbo.Accounts
		WHERE [ResourceId] = @NewPPE
		AND [AccountTypeId] IN (SELECT [Id] FROM PPEAccountTypes);

		INSERT INTO @CashPurchasesLineEntries([Index], 
			[DocumentLineIndex], [DocumentIndex], [EntryNumber], [Direction], [AccountId], [EntryTypeId], [Time], [Value])
		SELECT 0, DL.[Index], DL.DocumentIndex, LD.[EntryNumber], LD.[Direction], @A00, LD.[EntryTypeId], 5, 150000
		FROM @CashPurchasesLines DL
		JOIN dbo.LineDefinitions LD ON DL.LineDefinitionId = LD.[Id]


	END

-- for PPE: after defining the resource, we need to capture: Price
/*
	Dr. PPE
		Cr. Payable
	Dr. Payable
		Cr. Cash -- 

*/
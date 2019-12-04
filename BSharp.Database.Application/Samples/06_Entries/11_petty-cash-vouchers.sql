DECLARE @PettyCashVouchers dbo.DocumentList;
DECLARE @CashPurchasesLines dbo.LineList, @CashPurchasesLineEntries dbo.EntryList;
DECLARE @PettyCashVouchersWideLines [dbo].[WideLineList];

/*
Use Case 1: Accountant learns - say by email - that a user purchased a fixed asset, and paid for it from cash
Accountant can use a JV to record - directly in B# or on a paper pad first -: 
Dr. PPE
	Cr. VAT
	Cr. Cash
Requirements: Accountant needs to search for the PPE, VAT and Cash accounts, or even add them.
Risk: Choosing the wrong account (e.g., opening a duplicate).
Note that accountant would be signing on behalf of user who received the PPE
----------------------------------------
Use Case 2: Accountant receives from supplier an invoice with a Good Delivery Note. Accountant must prepare a check.
Accountant can add the GRN and sign on behalf of user, or forward it to (responsible to authorize and user to sign)
Then, accountant can prepare the cash payment, and forward to (responsible to authorize) and sign on behalf of suplier that the money was received
Then, accountant can file the document (to prevent any changes). Note that whatever lines were marked completed are taken into consideration for the balances.
Requirements: accountant enters the resource, etc.
----------------------------------------
Use Case 3: (Petty cash voucher) User records on system that he/she received goods from a supplier and paid for it from its own cash account
user enters all details, but accountant reviews/specifies the accounts
Tabs: Inventory & PPE => inventory {make expense, capitalize}, Services and Consumables => expenses {make inventory, capitalize}, Capitalization (expenses & inventory => PPE)
Description, Supplier, Invoice Date, Invoice Number, [Cash Acct], Total Paid, [VAT Acct], VAT included, {[Debit Acct], ResponsibilityCenter, Agent, Location, Item, Qty}
For the debit account, an accountant has to figure it out. Since even a service might be capitalized, and a good might be expensed immediately.
These are accounting concepts that require knowledge. So, while we allow the user to make a first guess, the accountant must be able to modify the accounts/purposes.

Petty Cash Purchases Tab
------------------------
Invoice Date, Description, Total Amount, VAT included, Supplier,			Invoice Number, Item,				Quantity,		Custodian,		Location,		Responsibility Center
RelatedDate1, Memo,		MoneyAmount3,	MoneyAmount2, RelatedAgent2/3, ExternalRef1/3, RelatedResource1, RelatedQuantity1, RelatedAgent1, RelatedLocation1, ResponsibilityCenter1
Accountant is responsible for Account 1: either select it or add new.
System suggests Account2: using VAT Account type. If there is none, accountant must define. If there is more than 1, accountant must specify.
System suggests Account3: using cash-on-hand account type, and custodian.

Cash Purchases Tab -- Payments Tab
-----------------------------------
*/

BEGIN
	INSERT INTO @PettyCashVouchers
	([Index],	[DocumentDate], [Memo]) VALUES
	--(0,			'2018.02.08',	N'Projector for Exec office'), -- fixed asset
	--(1,			'2018.02.15',	N'Fuel for machinery'), -- inventory
	--(2,			'2018.02.22',	N'HP laser jet ink + SQL Server 2019 License'); -- Consumables + Intangible
	(0,			'2018.02.08',	N'Petty Cash purchases/Alem Bayu/Feb 1 - Feb 8'),
	(1,			'2018.02.15',	N'Petty Cash purchases/Alem Bayu/Feb 9 - Feb 15'),
	(2,			'2018.02.22',	N'Petty Cash purchases/Alem Bayu/Feb 16 - Feb 22');

	INSERT INTO @PettyCashVouchersWideLines
	([Index], [DocumentIndex], [LineDefinitionId], [Memo]) VALUES
	(0,			0,				N'PettyCashPayment', N'Projector for Exec office'), 
	(1,			0,				N'PettyCashPayment', N'Fuel for car'),
	(2,			1,				N'PettyCashPayment', N'Cash machine yearly maintenance'),
	(3,			1,				N'PettyCashPayment', N'Fikadu Salary'),
	(4,			2,				N'PettyCashPayment', N'Employee Income Tax'),
	(5,			2,				N'PettyCashPayment', N'Jan VAT payment'),
	(6,			2,				N'PettyCashPayment', N'Office rent');

	EXEC [api].[Documents__Save]
		@DefinitionId = N'petty-cash-vouchers',
		@Documents = @PettyCashVouchers, @WideLines = @PettyCashVouchersWideLines,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Petty Cash Vouchers: Insert'
		GOTO Err_Label;
	END;

	select * from documents;

	IF @DebugPettyCashVouchers = 1
	BEGIN
		DECLARE @PettyCashVoucherIds dbo.IdList;
		INSERT INTO @PettyCashVoucherIds([Id]) SELECT [Id] FROM dbo.Documents WHERE [DefinitionId] = N'petty-cash-vocuhers';
		EXEC [rpt].[Docs__UI] @PettyCashVoucherIds;
	END
/*
	
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
			[LineIndex], [DocumentIndex], [EntryNumber], [Direction], [AccountId], [EntryClassificationId], [Time], [Value])
		SELECT 0, DL.[Index], DL.DocumentIndex, LD.[EntryNumber], LD.[Direction], @A00, LD.[EntryClassificationId], 5, 150000
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
*/
END
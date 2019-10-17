DECLARE @CashPurchases DocumentList, @CashPurchasesLines DocumentLineList;

BEGIN
	INSERT INTO @CashPurchases
	([Index],	[DocumentDate], [Memo]) VALUES
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
-- in first line, user creates new resource definition from list: {PPE},
	DECLARE @NewPPEList dbo.ResourceList;
	INSERT INTO @NewPPEList([Name], [ResourceClassificationId])
	VALUES(N'Epson T330 Projector', NULL);
	EXEC api.Resources__Save @DefinitionId = N'property-plant-and-equipment', @Entities = @NewPPEList, @ReturnIds = 1;
	DECLARE @NewPPE INT = (SELECT [Id] FROM dbo.Resources WHERE ResourceDefinitionId = N'property-plant-and-equipment' AND [Name] = N'Epson T330 Projector');

	DECLARE @A00 INT;
	-- look it up.  if not available, add it
	SELECT @A00 = [Id] FROM dbo.Accounts WHERE [ResourceId] = @NewPPE
-- for PPE: after defining the resource, we need to capture: Price
/*
	Dr. PPE
		Cr. Payable
	Dr. Payable
		Cr. Cash -- 

*/

INSERT INTO @WLSave ([LineIndex],
[DocumentIndex], [LineType], [AgentId1], [Reference1], [Amount1], [Amount2], [Reference2], [Amount3], [Reference3], [AgentId3], [AgentId2])   
			-- Supplier, Invoice #, Invoice Amount, Amount Withheld,	WT Ref,	Amount Paid,	Check Ref, Paid From, WT Entity
			-- Custody 1, Ref 1		Amount 1,		Amount 2,			Ref 2,	Amount 3,		Ref 3, Custody 3
VALUES
(@WLIdx + 1, @DIdx, N'PaymentIssueToSupplier',	@Lifan,	N'FS104', 200000, 4000, N'WT101', 196000, N'CK1201', @TigistSafe, @ERCA);

EXEC [api].[Documents__Save]
	@Documents = @DSave, @DocumentLineTypes = @DLTSave, @WideLines = @WLSave,
	@Lines = @LSave, @Entries = @ESave,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@DocumentsResultJson = @DResultJson OUTPUT, @LinesResultJson = @LResultJson OUTPUT, @EntriesResultJson = @EResultJson OUTPUT
DELETE FROM @DSave; DELETE FROM @DLTSave; DELETE FROM @WLSave; DELETE FROM @LSave; DELETE FROM @ESave;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Payment to Supplier: Save'
	GOTO Err_Label;
END;

DELETE FROM @Docs;
INSERT INTO @Docs([Index], [Id]) 
SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id] FROM dbo.Documents 
WHERE [State] = N'Draft';

EXEC [dbo].[api_Transactions__Post]
	@Documents = @Docs,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@ReturnIds = 0,
 	@DocumentsResultJson = @DResultJson OUTPUT,
	@LinesResultJson = @LResultJson OUTPUT,
	@EntriesResultJson = @EResultJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Payment to Supplier: Post'
	GOTO Err_Label;
END
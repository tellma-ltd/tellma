DECLARE @D_CPV [dbo].[DocumentList], @L_CPV [dbo].LineList, @E_CPV [dbo].EntryList, @D_CPVIds dbo.IdList;
DECLARE @WL_CPV [dbo].[WideLineList];

BEGIN
	INSERT INTO @D_CPV
	([Index],	[DocumentDate], [Memo]) VALUES
	--(0,			'2018.02.08',	N'Projector for Exec office',					@OS_Steel), -- fixed asset
	--(1,			'2018.02.15',	N'Fuel for machinery',							@OS_Steel), -- inventory
	(2,			'2018.02.22',	N'HP laser jet ink + SQL Server 2019 License'); -- Consumables + Intangible

	INSERT INTO @WL_CPV
	EXEC bll.LineDefinitionEntries__Pivot @index = 0, @DocumentIndex = 2, @DefinitionId = N'BankPayment';

	select * FROM @WL_CPV;
	--INSERT INTO @WL_CPV
	--([Index], [DocumentIndex], [DefinitionId],		[AgentId]) VALUES
	--(1,			2,				N'PurchaseInvoice',	@Amazon),
	--(2,			2,				N'PurchaseInvoice',	@Amazon);--,
	----(3,			2,				N'ManualLine');

	EXEC [api].[Documents__Save]
		@DefinitionId = N'cash-payment-vouchers',
		@Documents = @D_CPV, @WideLines = @WL_CPV, @Lines = @L_CPV, @Entries = @E_CPV,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Capital Investment (M): Insert'
		GOTO Err_Label;
	END;

	IF @DebugCashPaymentVouchers = 1
	BEGIN
			INSERT INTO @D_CPVIds([Id]) SELECT [Id] FROM dbo.Documents WHERE DefinitionId = N'cash-payment-vouchers';
			EXEC [rpt].[Docs__UI] @D_CPVIds;
	END

END
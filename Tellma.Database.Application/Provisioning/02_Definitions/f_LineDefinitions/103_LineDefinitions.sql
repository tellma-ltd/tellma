IF @DB = N'103'
BEGIN
	
	-- Withholding Tax Payable
	INSERT @LineDefinitions([Index],
	[Id],						[TitleSingular],			[TitleSingular2],		[TitlePlural],					[TitlePlural2],			[AgentDefinitionList], [ResponsibilityTypeList]) VALUES (
	4,N'WithholdingTaxPayable',	N'Withholding Tax Payable',	N'ضريبة خصم مشتريات',	N'Withholding Taxes Payable',	N'ضرائب خصم مشتريات',	N'suppliers',			N'Investment');
	UPDATE @LineDefinitions
	SET [Script] = N'
		SET NOCOUNT ON
		DECLARE @ProcessedWideLines WideLineList;

		INSERT INTO @ProcessedWideLines
		SELECT * FROM @WideLines;
		-----
		UPDATE @ProcessedWideLines
		SET
			[NotedAgentId0]				= [AgentId1],
			[MonetaryValue0]			= 0.02 * [NotedAmount0],
			[ResponsibilityCenterId1]	= [ResponsibilityCenterId0],
			[ExternalReference1]		= [ExternalReference0],
			[CurrencyId1]				= [CurrencyId0]
		-----
		SELECT * FROM @ProcessedWideLines;'
	WHERE [Index] = 4;
	INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Index],
	[Direction],	[AccountTypeParentId],	[AccountTagId]) VALUES
	(0,4,0,+1,		@TradeAndOtherPayables,	N'WHTX'),
	(1,4,1,-1,		@TradeAndOtherPayables,	N'TPBL');
	INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
	[SortKey],	[ColumnName],						[Label],					[Label2],				[RequiredState],
																										[ReadOnlyState]) VALUES
	(0,4,0,		N'Entry[0].NotedDate',				N'Date',					N'التاريخ',				1,4), 
	(1,4,1,		N'Line.Memo',						N'Memo',					N'البيان',				1,4),
	(2,4,2,		N'Entry[0].CurrencyId',				N'Currency',				N'العملة',				1,2), 
	(3,4,3,		N'Entry[1].AgentId',				N'Supplier',				N'المورد',				1,2), 
	(4,4,4,		N'Entry[0].NotedAmount',			N'TaxableAmount',			N'المبلغ الخاضع للخصم',1,2),
	(5,4,5,		N'Entry[0].MonetaryValue',			N'Withtholding Tax',		N'الخصم الضريبي',		0,4),
	(6,4,6,		N'Entry[0].ExternalReference',		N'Voucher #',				N'رقم الإيصال',			3,4),
	(7,4,7,		N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',	N'مركز المسؤولية',		4,4);

END
IF @DB = N'102'
BEGIN
	-- Withholding Tax Payable
	INSERT @LineDefinitions([Index],
	[Id],						[TitleSingular],			[TitleSingular2],		[TitlePlural],					[TitlePlural2],			[AgentDefinitionList]) VALUES (
	4,N'WithholdingTaxPayable',	N'Withholding Tax Payable',	N'ضريبة خصم مشتريات',	N'Withholding Taxes Payable',	N'ضرائب خصم مشتريات',	N'suppliers');
END
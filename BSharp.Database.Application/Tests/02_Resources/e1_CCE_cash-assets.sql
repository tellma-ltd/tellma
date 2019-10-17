	INSERT INTO dbo.ResourceDefinitions (
		[Id],			[TitlePlural],	[TitleSingular],	[ResourceTypeParentList]) VALUES
	( N'cash-assets',	N'Cash Assets',	N'Cash Asset',		N'CashAndCashEquivalent');
	
	DECLARE @CashAssets dbo.ResourceList;
	INSERT INTO @CashAssets ([Index],
	[ResourceTypeId],	[Name],			[MonetaryValueCurrencyId]) VALUES
	(0, N'Cash',		N'Cash/ETB',	N'ETB'),
	(1, N'Cash',		N'Cash/USD',	N'USD');

	EXEC [api].[Resources__Save]
		@DefinitionId = N'cash-assets',
		@Entities = @CashAssets,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Cash and cash equivalents'
		GOTO Err_Label;
	END;

IF @DebugResources = 1 
BEGIN
	SELECT  N'cash-assets' AS [Resource Definition]
	DECLARE @CashAssetIds dbo.IdList;
	INSERT INTO @CashAssetIds SELECT [Id] FROM dbo.Resources WHERE [ResourceDefinitionId] = N'cash-assets';
	
	SELECT ResourceTypeId, [Name] AS 'Cash Asset', [Currency]
	FROM rpt.Resources(@CashAssetIds);
END
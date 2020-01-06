IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
		[Id],			[TitlePlural],	[TitleSingular],[TimeUnitVisibility], [CurrencyVisibility]) VALUES 
	(	N'machineries',	N'Machineries',	N'Machinery',	 N'Required',			N'Optional');

	EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting'
		GOTO Err_Label;
	END;		
END
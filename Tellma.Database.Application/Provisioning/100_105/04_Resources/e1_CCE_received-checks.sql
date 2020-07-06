	INSERT INTO dbo.ResourceDefinitions (
	[Code],					[TitlePlural],		[TitleSingular]) VALUES
	( N'received-checks',	N'Received Checks',	N'Received Check');

	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],									[MonetaryValue]) VALUES
	(0,	N'Walia Steel Oct 2019 Check',	69000),
	(1,	N'Best Plastic Oct 2019 Check',	15700),
	(2,	N'Best Paint Oct 2019 Check',	6900);

	--INSERT INTO @ResourceUnits([Index], [HeaderIndex],
	--		[UnitId],						[Multiplier]) VALUES
	--(0, 0, dbo.fn_UnitName__Id(N'ea'),		1),
	--(0, 1, dbo.fn_UnitName__Id(N'ea'),		1),
	--(0, 2, dbo.fn_UnitName__Id(N'ea'),		1);

	EXEC [api].[Resources__Save] -- N'received-checks'
		@DefinitionId =  N'received-checks',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting received checks: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
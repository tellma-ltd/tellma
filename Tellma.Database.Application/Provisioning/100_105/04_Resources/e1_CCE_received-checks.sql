		DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],							[MonetaryValue]) VALUES
	(0,	N'Walia Steel Oct 2019 Check',	69000),
	(1,	N'Best Plastic Oct 2019 Check',	15700),
	(2,	N'Best Paint Oct 2019 Check',	6900);

	EXEC [api].[Resources__Save] -- N'received-checks'
		@DefinitionId =  @ReceivedCheckRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting received checks: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	DECLARE @R1 dbo.ResourceList, @RP1 dbo.ResourcePickList;
	
	INSERT INTO @R1 ([Index],
		[Name],			[Code],		[CurrencyId]) VALUES
	(0, N'Cash/ETB',	N'ETB',		N'ETB'), -- may not be needed. Implicit in Account
	(1, N'Cash/USD',	N'USD',		N'USD'); -- may not be needed. Implicit in Account
	EXEC [api].[Resources__Save] --  N'cash-and-cash-equivalents',
		@ResourceDefinitionId =  N'cash-and-cash-equivalents',
		@Resources = @R1,
	--	@Picks = @RP1,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Cash and cash equivalents'
		GOTO Err_Label;
	END;
DECLARE @R2 [dbo].ResourceList, @RP2 [dbo].ResourcePickList;

INSERT INTO @R2 ([Index],
		[Name],						[Code],		[CurrencyId]) VALUES
	(0,	N'Checks (received)/ETB',	N'RCKETB',	N'ETB'); -- may not be needed. Implicit in Account
	INSERT INTO @RP2([Index], [ResourceIndex],
		[ProductionDate],	[Code],		[MonetaryValue], [IssuingBankId]) VALUES
	(0,0,	N'2017.10.01',	N'101009',	6900,			@CBE),
	(1,0,	N'2017.10.15',	N'2308',	17550,			@AWB);	
	EXEC [api].[Resources__Save] -- N'received-checks'
	@ResourceDefinitionId =  N'received-checks',
	@Resources = @R2,
--	@Picks = @RP2,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting received checks'
		GOTO Err_Label;
	END;
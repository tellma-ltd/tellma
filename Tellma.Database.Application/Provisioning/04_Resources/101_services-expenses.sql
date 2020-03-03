IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[AccountTypeId],	[Name],					[Name2]) VALUES
	(0,	@ServicesExpense,	N'Monthly Subscription',N'اشتراك شهري'),
	(1, @ServicesExpense,	N'Yearly Support',		N'مساندة سنوية');

	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],	[Multiplier]) VALUES
	(0, 0,	@Month,		1),
	(0, 1,	@Year,		1);;

	EXEC [api].[Resources__Save] -- N'services-expenses'
		@DefinitionId = N'services-expenses',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting services: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	SELECT @MonthlySubscription = [Id] FROM dbo.Resources WHERE [Name] = N'Monthly Subscription';
END
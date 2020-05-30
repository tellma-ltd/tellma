IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	--INSERT INTO @Resources ([Index],
	--	[RevenueTypeId],				[Name],						[Name2]) VALUES
	--(0,	@RevenueFromRenderingOfServices, N'Monthly Subscription',	N'اشتراك شهري'),
	--(1, @RevenueFromRenderingOfServices, N'Yearly Support',			N'مساندة سنوية'),
	--(2, @RevenueFromRenderingOfServices, N'ERP Implementation',		N'تفعيل النظام'),
	--(3, @RevenueFromRenderingOfServices, N'ERP Stabilization',		N'استقرار النظام')	
		INSERT INTO @Resources ([Index],
		[Name],						[Name2]) VALUES
	(0,	N'Monthly Subscription',	N'اشتراك شهري'),
	(1, N'Yearly Support',			N'مساندة سنوية'),
	(2, N'ERP Implementation',		N'تفعيل النظام'),
	(3, N'ERP Stabilization',		N'استقرار النظام')	
	;

	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],	[Multiplier]) VALUES
	(0, 0,	@Month,		1),
	(0, 1,	@Year,		1),
	(0, 2,	@ea,		1),
	(0, 3,	@Month,		1);

	EXEC sys.sp_set_session_context 'UserId', @Jiad_akra;
	EXEC [api].[Resources__Save] -- N'services-expenses'
		@DefinitionId = @revenue_servicesDef,
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
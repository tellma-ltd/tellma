-- De-Activate Unneeded Entry Types


-- Activate Currencies in Use
INSERT INTO @IndexedCurrencyIds
([Index],	[Id]) VALUES
(0,			@SDG),
(1,			@USD);

EXEC [api].[Currencies__Activate]
	@IndexedIds = @IndexedCurrencyIds,
	@IsActive = 1,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies Activating: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

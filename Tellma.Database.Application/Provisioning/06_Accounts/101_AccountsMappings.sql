INSERT INTO @AccountMappings([Index],
	[DesignationId],		[AccountId]) VALUES
(3,@exchange_gain_lossADef,	@1ExchangeGainLoss),
(4,@exchange_varianceADef,	@1ExchangeVariance)

	EXEC [api].[AccountMappings__Save]
		@Entities = @AccountMappings,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Account Mappings: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
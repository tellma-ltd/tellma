	DECLARE @cashiers dbo.[CustodyList];
	DECLARE @petty_cash_funds dbo.[CustodyList];
	DECLARE @bank_accounts dbo.[CustodyList];
	
-- Cashiers
IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @cashiers
	([Index], [Name]) VALUES
	(0,		N'Cashier - Tigist');
EXEC [api].[Custodies__Save]
	@DefinitionId = @SafeCD,
	@Entities = @cashiers,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Cashiers: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
	-- Petty Cash Funds
IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @petty_cash_funds
	([Index], [Name]) VALUES
	(0,		N'Cashier');
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	Print N''
EXEC [api].[Custodies__Save]
	@DefinitionId = @SafeCD,
	@Entities = @petty_cash_funds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Petty Cash Funds: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

	EXEC [api].[Accounts__Save]
		@Entities = @Accounts,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Accounts: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @RJB_USD INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'RJB - USD' );
	DECLARE @RJB_SAR INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'RJB - SAR' );
	DECLARE @RJB_LC INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'RJB - LC' );
	DECLARE @MIT INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'TF1903950009' );
	DECLARE @WH INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Paper Warehouse' );

	DECLARE @5Capital INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Capital' );
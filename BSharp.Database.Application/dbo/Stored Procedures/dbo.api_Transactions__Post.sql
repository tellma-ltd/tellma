CREATE PROCEDURE [dbo].[api_Transactions__Post]
	@Entities [dbo].[IdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	-- if all documents are already posted, return
	IF NOT EXISTS(
		SELECT * FROM [dbo].[Documents]
		WHERE [Id] IN (SELECT [Id] FROM @Entities)
		AND [State] <> N'Posted'
	)
		RETURN;

	-- Sign the document before posting it
	EXEC [bll].[Documents_Filter__Sign]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents__Sign] @Entities = @Entities;

	-- Validate, checking available signatures for transaction type
	EXEC [dbo].[bll_Transactions_Validate__Post]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
			
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Documents_State__Update] @Entities = @Entities, @State = N'Posted';
END;
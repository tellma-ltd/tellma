CREATE PROCEDURE [api].[Accounts__Save]
	@Entities [dbo].[AccountList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ProcessedAccounts [dbo].[AccountList];

	INSERT INTO @ProcessedAccounts
	EXEC bll.[Accounts__Preprocess]
		@Entities = @Entities;

	-- Add here Code that is handled by C#
	
	EXEC [bll].[Accounts_Validate__Save]
		@Entities = @ProcessedAccounts,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Accounts__Save]
		@Entities = @ProcessedAccounts,
		@ReturnIds = @ReturnIds;
END;
CREATE PROCEDURE [api].[Contracts__Save]
	@DefinitionId INT,
	@Entities [ContractList] READONLY,
	@ContractUsers dbo.[ContractUserList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Contracts_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ContractUsers = @ContractUsers;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Contracts__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ContractUsers = @ContractUsers,
		@ReturnIds = @ReturnIds;
END
CREATE PROCEDURE [api].[Contracts__Save]
	@DefinitionId INT,
	@Entities [ContractList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#

	EXEC [bll].[Contracts_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Contracts__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END
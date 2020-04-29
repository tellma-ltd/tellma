CREATE PROCEDURE [api].[Contracts__Save]
	@DefinitionId INT,
	@Entities [ContractList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [bll].[Contracts_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities;

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
		@ReturnIds = @ReturnIds;
END
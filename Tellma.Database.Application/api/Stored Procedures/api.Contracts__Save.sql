CREATE PROCEDURE [api].[Contracts__Save]
	@DefinitionId INT,
	@Entities [ContractList] READONLY,
	@ContractUsers dbo.[ContractUserList] READONLY,
	@ReturnIds BIT = 0,
	@Top INT = 10,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors ValidationErrorList;
	-- Add here Code that is handled by C#

	-- Currency is required
	IF (
		SELECT [CurrencyVisibility]
		FROM dbo.ContractDefinitions
		WHERE [Id] = @DefinitionId
	) = N'Required'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0]) 
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCurrencyForContract0IsRequired',
		dbo.fn_Localize(FE.[Name],FE.[Name2], FE.[Name3]) AS ContractName
	FROM @Entities FE
	WHERE CurrencyId IS NULL;

	-- Center is required
	IF (
		SELECT [CenterVisibility]
		FROM dbo.ContractDefinitions
		WHERE [Id] = @DefinitionId
	) = N'Required'
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0]) 
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheCenterForContract0IsRequired',
		dbo.fn_Localize(FE.[Name],FE.[Name2], FE.[Name3]) AS ContractName
	FROM @Entities FE
	WHERE CenterId IS NULL;

	INSERT INTO @ValidationErrors
	EXEC [bll].[Contracts_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Entities = @Entities,
		@ContractUsers = @ContractUsers,
		@Top = @Top;

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
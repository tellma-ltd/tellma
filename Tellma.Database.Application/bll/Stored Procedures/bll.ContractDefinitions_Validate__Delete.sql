CREATE PROCEDURE [bll].[ContractDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that ContractDefinitionId is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheContractDefinitionIsUsedInAccountType0',
		dbo.fn_Localize(AD.[Name], AD.[Name2], AD.[Name3]) AS [Account]
	FROM @Ids FE
	JOIN dbo.[AccountTypeContractDefinitions] ADRD ON ADRD.[ContractDefinitionId] = FE.[Id]
	JOIN dbo.[AccountTypes] AD ON AD.[Id] = ADRD.[AccountTypeId]

	SELECT TOP(@Top) * FROM @ValidationErrors;
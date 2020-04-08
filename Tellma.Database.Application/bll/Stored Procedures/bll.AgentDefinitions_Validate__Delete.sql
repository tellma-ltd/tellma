CREATE PROCEDURE [bll].[AgentDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that AgentDefinitionId is not used in Account Types
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinitionIsUsedInAccountType0',
		dbo.fn_Localize(AC.[Name], AC.[Name2], AC.[Name3]) AS [Account]
	FROM @Ids FE
	JOIN dbo.AccountTypes AC ON AC.[AgentDefinitionId] = FE.[Id]

	SELECT TOP(@Top) * FROM @ValidationErrors;
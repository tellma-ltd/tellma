CREATE PROCEDURE [bll].[Accounts_Validate__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@Top INT = 10,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	IF @IsActive = 0 -- Attempt to deactivate
	BEGIN
		WITH
		ActiveAccounts([Index], [AccountId], [Quantity], [Value])
		AS (
			SELECT I.[Index], I.[Id] AS [AccountId],
				SUM(E.[Direction] * E.[BaseQuantity]) AS [Quantity],
				SUM(E.[Direction] * E.[Value]) AS [Value]
			FROM map.DetailsEntries() E
			JOIN dbo.Lines L ON E.[LineId] = L.[Id]
			JOIN @Ids I ON I.[Id] = E.[AccountId]
			WHERE L.[State] >= 0 -- N'Posted'
			GROUP BY I.[Index], I.[Id]
			HAVING
				SUM(E.[Direction] * E.[BaseQuantity]) <> 0 OR
				SUM(E.[Direction] * E.[Value]) <> 0
		)
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(AA.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasNonZeroQuantityBalance1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			CAST ([Quantity] AS NVARCHAR (255))
		FROM ActiveAccounts AA
		JOIN dbo.Accounts A ON AA.[AccountId] = A.[Id]
		WHERE [Quantity] <> 0
		UNION
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(AA.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasNonZeroValueBalance1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			CAST ([Value] AS NVARCHAR (255))
		FROM ActiveAccounts AA
		JOIN dbo.Accounts A ON AA.[AccountId] = A.[Id]
		WHERE [Quantity] = 0 AND [Value] <> 0
	END

	IF @IsActive = 1 -- Attempt to activate
	BEGIN
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInvisibleAgentDefinition1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.AgentDefinitions AD ON AD.[Id] = A.[AgentDefinitionId]
		WHERE AD.[State] <> N'Visible'

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInvisibleResourceDefinition1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.ResourceDefinitions RD ON RD.[Id] = A.[ResourceDefinitionId]
		WHERE RD.[State] <> N'Visible'

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInvisibleNotedAgentDefinition1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.AgentDefinitions AD ON AD.[Id] = A.[NotedAgentDefinitionId]
		WHERE AD.[State] <> N'Visible'

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInvisibleNotedResourceDefinition1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.ResourceDefinitions RD ON RD.[Id] = A.[NotedResourceDefinitionId]
		WHERE RD.[State] <> N'Visible'

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInActiveAgent1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](AG.[Name], AG.[Name2], AG.[Name3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.Agents AG ON AG.[Id] = A.[AgentId]
		WHERE AG.[IsActive] = 0

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInActiveResource1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.Resources R ON R.[Id] = A.[ResourceId]
		WHERE R.[IsActive] = 0

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInActiveNotedAgent1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](AG.[Name], AG.[Name2], AG.[Name3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.Agents AG ON AG.[Id] = A.[NotedAgentId]
		WHERE AG.[IsActive] = 0

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInActiveNotedResource1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.Resources R ON R.[Id] = A.[NotedResourceId]
		WHERE R.[IsActive] = 0

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST([Index] AS NVARCHAR (255)) + ']',
			N'Error_TheAccount0HasInActiveNotedResource1',
			[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]),
			[dbo].[fn_Localize](ET.[Name], ET.[Name2], ET.[Name3])
		FROM @Ids AA
		JOIN dbo.Accounts A ON AA.[Id] = A.[Id]
		JOIN dbo.EntryTypes ET ON ET.[Id] = A.[NotedResourceId]
		WHERE ET.[IsActive] = 0
	END

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;
CREATE PROCEDURE [dal].[Lines_LineKey__Update]
@Ids IdList READONLY
AS
	DECLARE @EmployeeBenefitInCash INT = dal.fn_LineDefinitionCode__Id(N'EmployeeBenefitInCash.M');
	DECLARE @EmployeeBenefitInCashAmended INT = dal.fn_LineDefinitionCode__Id(N'EmployeeBenefitInCashAmended.M');

	MERGE INTO dbo.LineDefinitionsAgentsResourcesCurrencies AS t
	USING (
		SELECT DISTINCT BL.[LineKey], @EmployeeBenefitInCash AS [LineDefinitionId], E.[NotedAgentId] AS [AgentId], E.[ResourceId], E.[CurrencyId]
		FROM @Ids FL
		JOIN dbo.Lines BL ON BL.[Id] = FL.[Id]
		JOIN dbo.Entries E ON E.[LineId] = BL.[Id]
		WHERE BL.[DefinitionId] IN (@EmployeeBenefitInCash, @EmployeeBenefitInCashAmended)
		AND E.[Index] = 0
	) AS s ON (
			t.[LineDefinitionId]	= s.[LineDefinitionId]
		AND t.[AgentId]				= s.[AgentId]
		AND t.[ResourceId]			= s.[ResourceId]
		AND	t.[CurrencyId]			= s.[CurrencyId]
	)
	WHEN NOT MATCHED THEN
	INSERT ([LineDefinitionId], [AgentId], [ResourceId], [CurrencyId])
	VALUES (s.[LineDefinitionId], s.[AgentId], s.[ResourceId], s.[CurrencyId]);

	WITH VLines AS (
		SELECT BL.[LineKey], @EmployeeBenefitInCash AS [LineDefinitionId], E.[NotedAgentId] AS [AgentId], E.[ResourceId], E.[CurrencyId]
		FROM @Ids FL
		JOIN dbo.Lines BL ON BL.[Id] = FL.[Id]
		JOIN dbo.Entries E ON E.[LineId] = BL.[Id]
		WHERE BL.[DefinitionId] IN (@EmployeeBenefitInCash, @EmployeeBenefitInCashAmended)
		AND E.[Index] = 0
	)
	UPDATE V
	SET [LineKey] = T.[Id]
	FROM VLines V
	JOIN LineDefinitionsAgentsResourcesCurrencies T
	ON T.[LineDefinitionId] = V.[LineDefinitionId]
	AND T.[AgentId] = V.[AgentId]
	AND T.[ResourceId] = V.[ResourceId]
	AND T.[CurrencyId] = V.[CurrencyId]
	WHERE [LineKey] IS NULL OR [LineKey] <> T.[Id];
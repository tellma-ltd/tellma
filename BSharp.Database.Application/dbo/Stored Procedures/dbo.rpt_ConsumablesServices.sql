CREATE PROCEDURE [dbo].[rpt_ConsumablesServices]
	@fromDate Date = '01.01.2020',
	@toDate Date = '01.01.2020'
AS
BEGIN
	WITH ExpenseJournal AS (
		SELECT
			J.[AgentId], J.[EntryTypeId], RC.[Name], RC.[Name2], RC.[Name3],
			SUM(J.[Direction] * J.[Value]) AS [Expense]
		FROM [dbo].[fi_Journal](@fromDate, @toDate) J
		JOIN dbo.[Agents] RC ON J.[AgentId] = RC.Id
		JOIN dbo.[AgentRelations] AR ON AR.AgentId = J.AgentId AND AR.AgentRelationDefinitionId =  J.[AgentRelationDefinitionId]
		WHERE AR.[CostCenterType] IN (
			N'CostUnit',
			N'Production',
			N'SellingAndDistribution',
			N'Administration',
			N'Service',
			N'Shared'
		)
		GROUP BY J.[AgentId], J.[EntryTypeId], RC.[Name], RC.[Name2], RC.[Name3]
	)
	SELECT * FROM ExpenseJournal
	PIVOT (
		SUM([Expense])
		FOR [EntryTypeId] IN (
			[CostUnit], [Production], [SellingAndDistribution], [Administration], [Service], [Shared]
		)
	) AS pvt;
END;
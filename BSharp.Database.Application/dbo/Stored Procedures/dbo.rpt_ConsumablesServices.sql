CREATE PROCEDURE [dbo].[rpt_ConsumablesServices]
	@fromDate Date = '01.01.2020',
	@toDate Date = '01.01.2020'
AS
BEGIN
	WITH ExpenseJournal AS (
		SELECT
			J.[AgentId], J.[EntryTypeId], CO.[Name], CO.[Name2], CO.[Name3],
			SUM(J.[Direction] * J.[Value]) AS [Expense]
		FROM [dbo].[fi_Journal](@fromDate, @toDate) J
		JOIN dbo.[Agents] CO ON J.[AgentId] = CO.Id AND J.[AgentDefinitionId] = CO.[DefinitionId]
		WHERE CO.[CostObjectType] IN (
			N'CostUnit',
			--N'CostCenter', -- replaced by the ones underneath
			N'Production', -- this would be absorbed but not exactly
			N'SellingAndDistribution',
			N'Administration',
			N'Service', -- this should have zero expense after re-allocation
			N'Shared' -- should have zero expense after re-allocation
		)
		GROUP BY J.[AgentId], J.[EntryTypeId], CO.[Name], CO.[Name2], CO.[Name3]
	)
	SELECT * FROM ExpenseJournal
	PIVOT (
		SUM([Expense])
		FOR [EntryTypeId] IN (
			[CostUnit], --[CostCenter], 
			[Production], [SellingAndDistribution], [Administration], [Service], [Shared]
		)
	) AS pvt;
END;
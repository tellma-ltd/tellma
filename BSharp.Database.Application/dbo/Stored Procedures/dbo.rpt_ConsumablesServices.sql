CREATE PROCEDURE [dbo].[rpt_ConsumablesServices]
	@fromDate Date = '01.01.2020',
	@toDate Date = '01.01.2020'
AS
BEGIN
	WITH ExpenseJournal AS (
		SELECT
			J.[ResponsibilityCenterId], J.[EntryTypeId], RC.[Name], RC.[Name2], RC.[Name3],
			SUM(J.[Direction] * J.[Value]) AS [Expense]
		FROM [dbo].[fi_Journal](@fromDate, @toDate) J
		JOIN dbo.[Agents] RC ON J.[ResponsibilityCenterId] = RC.Id
		WHERE J.[EntryTypeId] IN (
			N'CostOfSales',
			N'DistributionCosts',
			N'AdministrativeExpense',
			N'OtherExpenseByFunction'
		)
		GROUP BY J.[ResponsibilityCenterId], J.[EntryTypeId], RC.[Name], RC.[Name2], RC.[Name3]
	)
	SELECT * FROM ExpenseJournal
	PIVOT (
		SUM([Expense])
		FOR [EntryTypeId] IN (
			[CostOfSales], [DistributionCosts], [AdministrativeExpense], [OtherExpenseByFunction]
		)
	) AS pvt;
END;
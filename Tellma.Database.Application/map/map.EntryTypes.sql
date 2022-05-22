CREATE FUNCTION [map].[EntryTypes]()
RETURNS TABLE
AS
RETURN (
	-- TODO, persist ChildCount and ActiveChildCount similar to Account Types, and expand this
	SELECT Q.*,
	IIF(Q.[Concept] IN (
		N'CostOfSales',
		N'DistributionCosts',
		N'AdministrativeExpense',
		N'OtherExpenseByFunction',
		N'OtherGainsLosses',
		N'CapitalizationExpenseByNatureExtension'
		), 1, 0) As IsExpenseByFunction,
	CC.[ActiveChildCount],
	CC.ChildCount
	FROM [dbo].[EntryTypes] Q
	CROSS APPLY (
		SELECT COUNT(*) AS [ChildCount],
		SUM(IIF([IsActive]=1,1,0)) AS  [ActiveChildCount]	
		FROM [dbo].[EntryTypes]
		WHERE [Node].IsDescendantOf(Q.[Node]) = 1
	) CC 
);
CREATE PROCEDURE [dbo].[rpt_WSI_MerchandiseInTransit]
/*
Assumptions:
1) Any inventory account is mapped to Ifrs concepts: Inventories, NonCurrentinventories, or their descendants
2) All entries use a raw material resource. For balance migration, we need to add for every inventory account
	a resource called non-specified (for that account), and migrate balances to it.

*/
	@AsOfDate Date = '01.01.2020'
AS
BEGIN
	WITH
	Ifrs_MIT AS ( -- Typically, there is ONE such node only.
		SELECT [Node] 
		FROM dbo.[IfrsAccounts] WHERE [Id] IN(N'CurrentInventoriesInTransit')
	),
	InventoriesInTransitAccounts AS (
		SELECT A.[Id] FROM dbo.Accounts A
		JOIN dbo.[IfrsAccounts] I ON A.[IfrsAccountId] = I.[Id]
		WHERE I.[Node].IsDescendantOf((SELECT * FROM Ifrs_MIT))	= 1
	),
	Balances AS (
		SELECT
			J.ResourceId,
			SUM(J.[Direction] * J.[NormalizedMoneyAmount]) AS [MoneyAmount],
			SUM(J.[Direction] * J.[NormalizedMass]) AS [Mass],
			SUM(J.[Direction] * J.[NormalizedCount]) AS [Count]

		FROM [dbo].[fi_JournalDetails](NULL, @AsOfDate) J
		WHERE J.AccountId IN (SELECT Id FROM InventoriesInTransitAccounts)
		GROUP BY J.ResourceId
	)
	SELECT B.ResourceId, R.[Name], R.[Name2], R.[Name3], B.[MoneyAmount], B.[Mass], B.[Count]
	FROM dbo.Resources R 
	JOIN Balances B ON R.Id = B.ResourceId;
END;
GO;
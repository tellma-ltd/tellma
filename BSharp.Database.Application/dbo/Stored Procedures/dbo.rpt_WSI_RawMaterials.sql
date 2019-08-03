CREATE PROCEDURE [dbo].[rpt_WSI_RawMaterials]
/*
Assumptions:
1) Any inventory account is mapped to Ifrs concepts: Inventories, NonCurrentinventories, or their descendants
2) All entries use a raw material resource. For balance migration, we need to add for every inventory account
	a resource called non-specified (for that account), and migrate balances to it.

*/
	@fromDate Date = '01.01.2015', 
	@toDate Date = '01.01.2020'
AS
BEGIN
	WITH
	Ifrs_RM AS (
		SELECT [Node] 
		FROM dbo.[IfrsAccounts] WHERE [Id] IN(N'RawMaterials')
	),
	RawMaterialAccounts AS (
		SELECT A.[Id] FROM dbo.Accounts A
		JOIN dbo.[IfrsAccounts] I ON A.[IfrsAccountId] = I.[Id]
		WHERE I.[Node].IsDescendantOf((SELECT * FROM Ifrs_RM))	= 1
	),
	OpeningBalances AS (
		SELECT
			J.ResourceId,
			SUM(J.[NormalizedCount] * J.[Direction]) AS [Count],
			SUM(J.[NormalizedMass] * J.[Direction]) AS [Mass]
		FROM [dbo].[fi_JournalDetails](NULL, @fromDate) J
		WHERE J.AccountId IN (SELECT Id FROM RawMaterialAccounts)
		GROUP BY J.ResourceId
	),
	Movements AS (
		SELECT
			J.ResourceId,
			SUM(CASE WHEN J.[Direction] > 0 THEN J.[NormalizedCount] ELSE 0 END) AS CountIn,
			SUM(CASE WHEN J.[Direction] > 0 THEN J.[NormalizedMass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN J.[Direction] < 0 THEN J.[NormalizedCount] ELSE 0 END) AS CountOut,			
			SUM(CASE WHEN J.[Direction] < 0 THEN J.[NormalizedMass] ELSE 0 END) AS MassOut
		FROM [dbo].[fi_JournalDetails](@fromDate, @toDate) J
		WHERE J.AccountId IN (SELECT Id FROM RawMaterialAccounts)
		GROUP BY J.ResourceId
	),
	RawMaterialsRegsiter AS (
		SELECT COALESCE(OpeningBalances.ResourceId, Movements.ResourceId) AS ResourceId,
			ISNULL(OpeningBalances.[Count],0) AS OpeningCount, 
			ISNULL(Movements.[CountIn],0) AS CountIn, ISNULL(Movements.[CountOut],0) AS CountOut,
			ISNULL(OpeningBalances.[Count], 0) + ISNULL(Movements.[CountIn], 0) - ISNULL(Movements.[CountOut],0) AS EndingCount,

			ISNULL(OpeningBalances.[Mass],0) AS OpeningMass, 
			ISNULL(Movements.[MassIn],0) AS MassIn, ISNULL(Movements.[MassOut],0) AS MassOut,
			ISNULL(OpeningBalances.[Mass], 0) + ISNULL(Movements.[MassIn], 0) - ISNULL(Movements.[MassOut],0) AS EndingMass
		FROM OpeningBalances
		FULL OUTER JOIN Movements ON OpeningBalances.ResourceId = Movements.ResourceId
	)
	SELECT RMR.ResourceId, R.[Name], R.[Name2],
		RMR.OpeningCount, RMR.OpeningMass,
		RMR.CountIn, RMR.MassIn, RMR.CountOut, RMR.MassOut,
		RMR.EndingCount, RMR.EndingMass
	FROM dbo.Resources R 
	JOIN RawMaterialsRegsiter RMR ON R.Id = RMR.ResourceId;
END;
GO;
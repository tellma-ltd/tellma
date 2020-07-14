CREATE PROCEDURE [rpt].[FinishedGoods__TrialBalance]
	@FromDate Date = '01.01.2020',
	@ToDate Date = '01.01.2020'
AS
-- WARNING: Useful only when all the FG accounts have HasResource = 1
BEGIN
	WITH FinishedGoodAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.AccountTypeId = ATC.Id
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATC.[Node]) = 1
		WHERE ATP.[Concept] IN (N'CurrentInventoriesHeldForSale')
	),
	JournalSummary AS (
		SELECT ResourceId,
			SUM(OpeningQuantity) AS OpeningQuantity, SUM(QuantityIn) AS QuantityIn, SUM(QuantityOut) AS QuantityOut, SUM(EndingQuantity) AS EndingQuantity,
			SUM(OpeningMass) AS OpeningMass, SUM(MassIn) AS MassIn, SUM(MassOut) AS MassOut, SUM(EndingMass) AS EndingMass
		FROM [map].[SummaryEntries](
			@FromDate,
			@ToDate
		)
		WHERE AccountId IN (SELECT [Id] FROM FinishedGoodAccounts)
		GROUP BY ResourceId
	)
	SELECT JS.*, R.[Code], R.[Name], R.[Name2], R.[Name3]
	FROM JournalSummary JS
	JOIN dbo.Resources R ON JS.ResourceId = R.Id
END;
GO;
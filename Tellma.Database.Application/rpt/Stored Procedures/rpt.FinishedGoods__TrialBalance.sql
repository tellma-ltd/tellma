CREATE PROCEDURE [rpt].[FinishedGoods__TrialBalance]
	@FromDate Date = '01.01.2020',
	@ToDate Date = '01.01.2020',
	@CenterId INT = NULL
AS
-- WARNING: Useful only when all the FG accounts have HasResource = 1
BEGIN
	WITH JournalSummary
	AS (
		SELECT ResourceId,
			SUM(OpeningQuantity) AS OpeningQuantity, SUM(QuantityIn) AS QuantityIn, SUM(QuantityOut) AS QuantityOut, SUM(EndingQuantity) AS EndingQuantity,
			SUM(OpeningMass) AS OpeningMass, SUM(MassIn) AS MassIn, SUM(MassOut) AS MassOut, SUM(EndingMass) AS EndingMass
		FROM [map].[SummaryEntries](
			@FromDate,
			@ToDate,
			@CenterId,
			N'FinishedGoods'
		)
		GROUP BY ResourceId
	)
	SELECT JS.*, R.[Code], R.[Name], R.[Name2], R.[Name3]
	FROM JournalSummary JS
	JOIN dbo.Resources R ON JS.ResourceId = R.Id
END;
GO;
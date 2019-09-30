CREATE PROCEDURE [rpt].[AccountClassifications__TrialBalance]
	@fromDate Date = '01.01.2019',
	@ToDate Date = '01.01.2020'
AS
	WITH JournalSummary
	AS (
		SELECT [AccountClassificationId],  SUM([Opening]) AS [Opening], SUM([Debit]) AS [Debit], SUM([Credit]) AS [Credit], SUM([Closing]) AS Closing
		FROM rpt.fi_JournalSummary(
			NULL, -- @AccountTypeList
			@fromDate,
			@ToDate, 
			NULL, --@MassUnitId
			NULL -- @CountUnitId
		)
		GROUP BY [AccountClassificationId]
	)
	SELECT JS.*, GLA.[Code], GLA.[Name], GLA.[Name2], GLA.[Name3]
	FROM JournalSummary JS
	JOIN dbo.[AccountClassifications] GLA ON JS.[AccountClassificationId] = GLA.Id
GO;
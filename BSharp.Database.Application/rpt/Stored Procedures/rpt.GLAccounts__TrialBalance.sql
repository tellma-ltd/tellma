CREATE PROCEDURE [rpt].[GLAccounts__TrialBalance]
	@fromDate Date = '01.01.2019',
	@ToDate Date = '01.01.2020'
AS
	WITH JournalSummary
	AS (
		SELECT GLAccountId,  SUM([Opening]) AS [Opening], SUM([Debit]) AS [Debit], SUM([Credit]) AS [Credit], SUM([Closing]) AS Closing
		FROM rpt.fi_JournalSummary(
			NULL, -- @AccountDefinitionId
			N'', -- @GLAccountsCode
			@fromDate,
			@ToDate, 
			NULL, --@MassUnitId
			NULL -- @CountUnitId
		)
		GROUP BY GLAccountId
	)
	SELECT JS.*, GLA.[Code], GLA.[Name], GLA.[Name2], GLA.[Name3]
	FROM JournalSummary JS
	JOIN dbo.GLAccounts GLA ON JS.GLAccountId = GLA.Id
GO;
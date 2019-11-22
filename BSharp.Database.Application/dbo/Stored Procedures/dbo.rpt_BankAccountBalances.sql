CREATE PROCEDURE [dbo].[rpt_BankAccountBalances]
	@FromDate Date = '01.01.2020',
	@ToDate Date = '01.01.2020'
AS
BEGIN
	WITH JournalSummary -- ReportDefinition
	AS (
		SELECT [AccountClassificationId], AccountId,
			-- Measures
			SUM([Opening]) AS [Opening], SUM([Debit]) AS [Debit], SUM([Credit]) AS [Credit], SUM([Closing]) AS Closing
		FROM rpt.fi_JournalSummary (
				N'BalancesWithBanks', -- @AccountTypeList
				@FromDate,
				@ToDate,
				NULL, -- @CountUnitId
				NULL, -- @MassUnitId,
				NULL -- @VolumneUnitId
			)
		GROUP BY [AccountClassificationId], AccountId -- rows
	)
	SELECT JS.*, A.[Code], A.[Name], A.[Name2], A.[Name3], A.[PartyReference]
	FROM JournalSummary JS
	JOIN dbo.Accounts A ON JS.AccountId = A.Id
END;
GO;
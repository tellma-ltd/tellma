CREATE FUNCTION [rpt].[Accounts__TrialBalance] (
-- useful for legacy reconciliation, with the assumption that accounts that are used by the legacy system for auto posting (e.g., 
-- trade debtors, trade creditors, and inventory assets/revenues/expense accounts) are NOT used for manual posting as well.
-- If they need it for manual posting, they need to defined G/L accounts in both
	@fromDate Date = '01.01.2019',
	@ToDate Date = '01.01.2020'
) RETURNS TABLE
AS 
RETURN
	WITH JournalSummary
	AS (
		SELECT [AccountClassificationId], 
			AccountId,
			SUM([Opening]) AS [Opening], SUM([Debit]) AS [Debit], SUM([Credit]) AS [Credit], SUM([Closing]) AS Closing
		FROM rpt.fi_JournalSummary(
			NULL, -- @AccountTypeList
			@fromDate,
			@ToDate, 
			NULL, -- @CountUnitId
			NULL, --@MassUnitId
			NULL -- @VolumneId
		)
		GROUP BY [AccountClassificationId], AccountId
	)
	SELECT AccountClassificationId, AccountId, [Opening], [Debit], [Credit], Closing
	FROM JournalSummary;
GO
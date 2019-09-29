CREATE PROCEDURE [dbo].[rpt_BankAccountBalances]
	@FromDate Date = '01.01.2020',
	@ToDate Date = '01.01.2020',
	@GLAccountCodeList NVARCHAR(MAX) = NULL
AS
BEGIN
	--WITH GLAccountCodes([Code]) AS (
	--	SELECT Value  
	--	FROM STRING_SPLIT(@GLAccountCodeList, ',')  
	--	WHERE RTRIM(Value) <> ''
	--),
	--GLAccountList([Id]) AS (
	--	SELECT [Id] FROM dbo.GLAccounts GLA
	--	JOIN GLAccountCodes GLC ON GLA.[Code] LIKE [GLC].[Code] + '%'
	--),
	--BankAccounts(AccountId) AS (
	--	SELECT [Id] FROM dbo.[Accounts]
	--	WHERE [AccountDefinitionId] = N'balances-with-banks'
	--	OR [GLAccountId] IN (
	--		SELECT [Id] FROM GLAccountList
	--	)
	--)
	WITH JournalSummary -- ReportDefinition
	AS (
		SELECT GLAccountId, AccountId,
			-- Measures
			SUM([Opening]) AS [Opening], SUM([Debit]) AS [Debit], SUM([Credit]) AS [Credit], SUM([Closing]) AS Closing
		FROM rpt.fi_JournalSummary (
				N'balances-with-banks', -- @AccountDefinitionId
				@GLAccountCodeList, --N'1100', -- @GLAccountsCode, must define manually
				@FromDate,
				@ToDate, 
				NULL, -- @MassUnitId,
				NULL -- @CountUnitId
			)
		GROUP BY GLAccountId, AccountId -- rows
	)
	SELECT JS.*, A.[Code], A.[Name], A.[Name2], A.[Name3], A.[PartyReference]
	FROM JournalSummary JS
	JOIN dbo.Accounts A ON JS.AccountId = A.Id
END;
GO;
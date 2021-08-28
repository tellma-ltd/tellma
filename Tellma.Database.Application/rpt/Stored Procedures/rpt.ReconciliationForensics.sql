CREATE PROCEDURE [rpt].[ReconciliationForensics]
AS
WITH UnreconciledEntries AS (
	SELECT L.PostingDate, E.[Direction], E.[MonetaryValue], E.[AgentId]
	FROM dbo.Lines L
	JOIN dbo.Entries E ON E.[LineId] = L.Id
	WHERE E.Id NOT IN (SELECT EntryId FROM dbo.ReconciliationEntries)
	AND L.[State] = 4
	AND E.AccountId IN (
		SELECT A.[Id] FROM dbo.Accounts A
		JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
		WHERE AC.[Concept] = N'BalancesWithBanks'
	)
),
UnreconciledExternalEntries AS (
	SELECT PostingDate, [Direction], [MonetaryValue], [AgentId]
	FROM dbo.ExternalEntries
	WHERE [Id] NOT IN (SELECT ExternalEntryId FROM dbo.ReconciliationExternalEntries)
) 
SELECT UE.PostingDate AS BookDate, UEE.PostingDate AS BankDate, UE.Direction * UE.MonetaryValue AS Amount,
--UE.@AccountIdId AS BookAccountId, UEE.@AccountIdId AS BankAccountId,
CE.[Name] AS BookAccount, CEE.[Name] AS BankAccount
FROM UnreconciledEntries UE
JOIN UnreconciledExternalEntries UEE ON ABS(UE.MonetaryValue - UEE.MonetaryValue) < 0.6
AND UE.[Direction] = UEE.Direction
AND UE.[AgentId] <> UEE.[AgentId]
AND DATEDIFF(MONTH, UE.PostingDate, UEE.PostingDate) = 0
JOIN dbo.[Agents] CE ON UE.[AgentId] = CE.[Id]
JOIN dbo.[Agents] CEE ON UEE.[AgentId] = CEE.[Id];
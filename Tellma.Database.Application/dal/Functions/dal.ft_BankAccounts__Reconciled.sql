CREATE FUNCTION [dal].[ft_BankAccounts__Reconciled] (
  @FromDate    DATE = NULL
)
RETURNS @returntable TABLE
(
  [PostingDate] DATE,
  [AgentId0] INT,
  [Quantity0]  INT
)
AS
BEGIN
--  SET @FromDate = ISNULL(@FromDate, CAST(GETDATE() AS DATE));

  WITH BankAccounts AS (
    SELECT [Id] FROM dbo.Accounts
    WHERE AccountTypeId IN (
      SELECT [Id] FROM dbo.AccountTypes
      WHERE [Concept] = N'BalancesWithBanks'
    )
  )
  INSERT @returntable([PostingDate], [AgentId0], [Quantity0])
  SELECT DATEFROMPARTS(Year(R.CreatedAt), Month(R.CreatedAt), 1),
    RL.[Id] AS BankAccount,
    ReconciledCount = COUNT(DISTINCT R.[Id])
  FROM dbo.Reconciliations R
  JOIN dbo.ReconciliationExternalEntries REE ON R.[Id] = REE.ReconciliationId
  JOIN dbo.ExternalEntries EE ON REE.[ExternalEntryId] = EE.[Id]
  JOIN dbo.[Agents] RL ON EE.[AgentId] = RL.[Id]
  WHERE EE.[AccountId] IN (SELECT [Id] FROM BankAccounts)
  AND (@FromDate IS NULL OR R.[CreatedAt] >= @FromDate)
  GROUP BY RL.[Id], Year(R.CreatedAt),Month(R.CreatedAt)
  ORDER BY Year(R.CreatedAt), Month(R.CreatedAt);  
  
  RETURN
END
GO
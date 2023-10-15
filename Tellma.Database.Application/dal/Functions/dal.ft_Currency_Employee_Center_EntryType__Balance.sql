CREATE FUNCTION [dal].[ft_Currency_Employee_Center_EntryType__Balance]
(
	@CurrencyId NCHAR (3),
	@EmployeeId INT,
	@CenterId INT,
	@EntryTypeId INT,
	@NotedDate DATE
)
RETURNS @returntable TABLE
(
  [EmployeeId] INT,
  [MonetaryValue] DECIMAL (19, 6),
  [CurrencyId] NCHAR (3),
  [NotedDate] DATE,
  [CenterId] INT
)

AS BEGIN
	INSERT INTO @returntable([EmployeeId],[MonetaryValue], [CurrencyId], [NotedDate], [CenterId])
	SELECT E.AgentId AS EmployeeId, SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue], E.[CurrencyId],
	E.[NotedDate], E.[CenterId]
	FROM dbo.Entries E
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON E.AccountId = A.[Id]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	WHERE L.[State] = 4
	AND AC.[Concept] = 'ShorttermEmployeeBenefitsAccruals'
	AND E.[CurrencyId] = @CurrencyId
	AND (@EmployeeId IS NULL OR E.[AgentId] = @EmployeeId)
	AND (@CenterId IS NULL OR E.[CenterId] = @CenterId)
	AND (@EntryTypeId IS NULL OR E.[EntryTypeId] = @EntryTypeId)
	AND (@NotedDate IS NULL	AND [NotedDate] IS NULL OR E.[NotedDate] <= @NotedDate)
	GROUP BY E.[CenterId], E.[CurrencyId], E.[AgentId], E.[NotedDate]
	HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	RETURN
END
GO
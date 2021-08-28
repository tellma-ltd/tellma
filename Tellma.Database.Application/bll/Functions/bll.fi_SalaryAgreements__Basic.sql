CREATE FUNCTION [bll].[fi_SalaryAgreements__Basic]()
RETURNS TABLE AS RETURN
(
	SELECT E.[LineId], E.[AgentId] AS [EmployeeId], E.[CurrencyId],
	E.[MonetaryValue] AS [BasicSalary], E.[Time1] AS AsOf,
	COALESCE(E.[Time2],
			DATEADD(DAY, -1, LEAD(E.[Time1]) OVER(PARTITION BY E.[AgentId] ORDER BY E.[Time1])),
			N'9999.12.31') AS Till
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.AccountTypes AC ON A.AccountTypeId = AC.[Id]
	JOIN dbo.Resources R ON E.[ResourceId] = R.[Id]
	WHERE AC.[Concept] = N'WagesAndSalaries'
	AND R.[Code] = N'BasicSalary'
	AND L.[State] = 2
);
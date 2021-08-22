CREATE FUNCTION [bll].[fi_SalaryAgreements__Basic]()
RETURNS TABLE AS RETURN
(
	SELECT E.[LineId], E.[RelationId] AS [EmployeeId], E.[CurrencyId],
	E.[MonetaryValue] AS [BasicSalary], E.[Time1] AS AsOf,
	COALESCE(E.[Time2],
			DATEADD(DAY, -1, LEAD(E.[Time1]) OVER(PARTITION BY E.[RelationId] ORDER BY E.[Time1])),
			N'9999.12.31') AS Till
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	WHERE E.[Index] = 0
	AND L.[State] = 2
	AND L.DefinitionId =  (
		SELECT [Id] FROM LineDefinitions WHERE [Code] = N'EmployeePeriodOfTimeServiceInvoiceTemplate'
	)
);
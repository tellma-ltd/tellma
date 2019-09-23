CREATE FUNCTION [bll].[fi_EmployeesIncomeTaxes]
(
	@EmployeeTaxableIncomes dbo.AgentBalanceList READONLY
)
RETURNS TABLE AS RETURN
(
	SELECT AgentId,
		(CASE 
			WHEN Balance < 350 THEN 0 
			WHEN Balance >= 350 AND Balance < 700 THEN 0.2 * Balance
			ELSE Balance * 0.3 END
		) AS [EmployeeIncomeTax]
	FROM @EmployeeTaxableIncomes
)

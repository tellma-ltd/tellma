CREATE FUNCTION [dal].[fn_Employee_Month__IsSalaryPaid] (
	@EmployeeId INT,
	@StartOfMonth DATE
)
RETURNS BIT
AS BEGIN
	DECLARE @StartOfNextMonth DATE = DATEADD(MONTH, 1, @StartOfMonth)
	IF EXISTS(
		SELECT *
		FROM dbo.Entries E
		JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		WHERE LD.[LineType] = 100
		AND L.[State] >= 0 -- in case salaries were prepared already but not posted
		AND R.[Code] = N'BasicSalary'
		AND AC.[Concept] = N'WagesAndSalaries'
		AND L.[PostingDate] >= @StartOfMonth
		AND L.[PostingDate] < @StartOfNextMonth
		AND E.[NotedAgentId] = @EmployeeId
	)
	RETURN 1;
	RETURN 0;
END
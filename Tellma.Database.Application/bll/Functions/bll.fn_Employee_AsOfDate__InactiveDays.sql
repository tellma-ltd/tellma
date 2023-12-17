CREATE FUNCTION [bll].[fn_Employee_AsOfDate__InactiveDays]
(
	@EmployeeId INT,
	@AsOfDate DATE
)
RETURNS INT
BEGIN

RETURN ISNULL((
	SELECT SUM(E.[Direction] * (DATEDIFF(DAY,
		E.[Time1],
		IIF(E.[Time2] <= @AsOfDate, E.[Time2], @AsOfDate)
		) + 1)) AS Balance
	FROM dbo.Lines L
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	WHERE L.[State] = 4
	AND AC.[Concept] = N'HRExtension'
	AND R.[Code] IN (N'UnpaidLeave', N'UnauthorizedLeave')
	AND E.[AgentId] = @EmployeeId
	AND E.[Time1] <= @AsOfDate
	), 0)
END
GO
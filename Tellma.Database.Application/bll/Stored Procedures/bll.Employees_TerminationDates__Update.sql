CREATE PROCEDURE [bll].[Employees_TerminationDates__Update]
-- Those who were mistakenly given an expiry date when they are meant to be renewed after expiration
@EmployeeId INT
AS
DECLARE @EmployeeAD INT = dal.fn_AgentDefinitionCode__Id(N'Employee');
DECLARE @ToDate DATE = (SELECT ToDate FROM dbo.Agents WHERE DefinitionId = @EmployeeAD AND [Id] = @EmployeeId)
DECLARE @Amount DECIMAL (19, 6), @Time1 DATE, @Time2 DATE;

IF @ToDate IS NOT NULL
BEGIN
	SELECT @Amount= -SUM(E.[Direction] * E.[MonetaryValue]), @Time1 = [Time1], @Time2 = [Time2]
	FROM dbo.Lines L
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE LD.[LineType] = 80
	AND AC.[Concept] = N'ShorttermEmployeeBenefitsAccruals'
	AND L.[State] = 2
	AND L.[EmployeeId] = @EmployeeId
	GROUP BY [Time1], [Time2]
END

PRINT @Amount;

SELECT L.PostingDate, LD.[TitleSingular], E.[Id], E.[AccountId], A.[Name], [Time1], [Time2]
	FROM dbo.Lines L
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE LD.[LineType] = 80
	AND L.[State] = 2
	AND L.[EmployeeId] = @EmployeeId
	Order by L.[PostingDate], LD.[TitleSingular]

IF @ToDate IS NOT NULL AND ISNULL(@Amount, 0) <> 0
BEGIN
	Update E
	SET E.[Time2] = NULL
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE LD.[LineType] = 80
	AND L.[State] = 2
	AND L.[EmployeeId] = @EmployeeId;

	UPDATE D
	SET D.[Time2] = NULL
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE LD.[LineType] = 80
	AND L.[State] = 2
	AND L.[EmployeeId] = @EmployeeId
	AND D.[Time2] IS NOT NULL;

	UPDATE Agents
	SET ToDate = NULL
	WHERE [Id] = @EmployeeId;
END
GO

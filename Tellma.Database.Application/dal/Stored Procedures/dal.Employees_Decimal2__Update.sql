CREATE PROCEDURE [dal].[Employees_Decimal2__Update]
AS
DECLARE @PostingDate DATE = GETDATE();

DECLARE @AnnualLeaveRS INT = dal.fn_ResourceDefinition_Code__Id(N'LeaveTypes', N'AnnualLeave');
DECLARE @EmpBenAnnualLeaveRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'AnnualLeave');
DECLARE @CurrentYearEnd DATE = DATEFROMPARTS(YEAR(@PostingDate), 12, 31); -- 31/12/2024
DECLARE @CurrentYearStart DATE = DATEFROMPARTS(YEAR(@PostingDate), 1, 1); -- 1/1/2024
DECLARE @EmployeeAD INT = dal.fn_AgentDefinitionCode__Id(N'Employee');
DECLARE @Widelines WidelineList;
DECLARE @EmployeeIds TABLE ([EmployeeId] INT PRIMARY KEY);

INSERT INTO @EmployeeIds ([EmployeeId])
SELECT [Id]
FROM dbo.Agents AG
WHERE DefinitionId = @EmployeeAD
AND [FromDate] <= @PostingDate
AND ([ToDate] IS NULL OR [ToDate] > @CurrentYearEnd)
AND [IsActive] = 1
AND [Code] <> N'0'
--Select * From @EmployeeIds;

DECLARE @TillYearEndDate TABLE ([EmployeeId] INT PRIMARY KEY, [RequestedLeaveDays] DECIMAL (19, 6) DEFAULT (0));
INSERT INTO @TillYearEndDate 
	SELECT E.[AgentId], SUM([Direction] * 
		IIF(E.[Time2] <= @CurrentYearEnd, [Quantity], DATEDIFF(DAY, E.[Time1], @CurrentYearEnd) + 1)
	) AS RequestedLeaveDays
	FROM dbo.Entries E
	JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN @EmployeeIds UP ON UP.[EmployeeId] = E.[AgentId]
		AND E.[Time1] <= @CurrentYearEnd
	WHERE AC.[Concept] = N'HRExtension' 
	AND E.[ResourceId] = @AnnualLeaveRS
	AND E.Direction = 1
	AND L.[State] >= 0 -- back to 0, based on request from HR director 2024-04-14
	GROUP BY E.[AgentId]
--Select * From @TillYearEndDate

DECLARE @LeaveAdjustments TABLE ([EmployeeId] INT PRIMARY KEY, [AdjustedLeaveDays] DECIMAL (19, 6) DEFAULT (0));
INSERT INTO @LeaveAdjustments 
	SELECT E.[AgentId], SUM([Direction] * [Quantity]) AS AdjustedLeaveDays
	FROM dbo.Entries E
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN @EmployeeIds UP ON UP.[EmployeeId] = E.[AgentId]
	WHERE AC.[Concept] = N'CurrentProvisionsForEmployeeBenefits' 
	AND E.[ResourceId] = @EmpBenAnnualLeaveRS
	AND LD.[Code] IN (N'ToHRFromCurrentProvisions',
			N'ToCurrentProvisionsForEmployeeBenefitsWithOtherShorttermEmployeeBenefitsFromEmployeeBenefitsAccruals')
	AND L.[State] = 4
	GROUP BY E.[AgentId]
--Select * From @LeaveAdjustments

UPDATE Agents
SET	[Decimal2] = dbo.fn_ActiveDates__AccruedLeaveDays(AG.[FromDate], @CurrentYearEnd, AG.[Int2],
	[bll].[fn_Employee_AsOfDate__InactiveDays](UP.[EmployeeId], @CurrentYearEnd)
)
			- ISNULL(YE.RequestedLeaveDays, 0)
			- ISNULL(LA.AdjustedLeaveDays, 0)
FROM dbo.Agents AG
JOIN @EmployeeIds UP ON UP.[EmployeeId] = AG.[Id]
LEFT JOIN @TillYearEndDate YE ON YE.[EmployeeId] = UP.[EmployeeId]
LEFT JOIN @LeaveAdjustments LA ON LA.[EmployeeId] = UP.[EmployeeId];


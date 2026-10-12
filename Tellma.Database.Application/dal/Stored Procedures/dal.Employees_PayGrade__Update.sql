CREATE PROCEDURE dal.Employees_PayGrade__Update -- used in 111
AS
BEGIN
	DECLARE @EmployeeAD INT = dal.fn_AgentDefinitionCode__Id(N'Employee');
	DECLARE @EmployeeGroupAD INT = dal.fn_AgentDefinitionCode__Id(N'EmployeeGroup');
	DECLARE @EmployeesDates dbo.IdDateList 
	INSERT INTO @EmployeesDates
	SELECT Id,GetDate() 
	FROM Agents
	WHERE DefinitionId = @EmployeeAD
	AND [Code] <> '0'
	AND [IsActive] = 1
	AND [Agent1Id] IS NOT NULL;

	UPDATE GP
	  SET GP.[Agent2Id] = SS.[PayGradeId]
	FROM dbo.Agents GP 
	JOIN dbo.Agents EMP ON EMP.[Agent1Id] = GP.[Id]
	JOIN dal.ft_EmployeesDates__EmployeesProfiles(@EmployeesDates) SS ON SS.[EmployeeId] = EMP.[Id]
	WHERE SS.[PayGradeId] IS NOT NULL
	AND EMP.DefinitionId = @EmployeeAD
	AND GP.DefinitionId = @EmployeeGroupAD
	AND GP.[Agent2Id] <> SS.[PayGradeId];
END
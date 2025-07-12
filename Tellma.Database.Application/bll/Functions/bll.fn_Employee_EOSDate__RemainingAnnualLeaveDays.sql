CREATE FUNCTION [bll].[fn_Employee_EOSDate__RemainingAnnualLeaveDays]
(
	@EmployeeId INT,
	@EndOfServiceDate DATE,
	@YearlyAccrual INT = 21,
	@InactiveDays INT = 0
)
RETURNS DECIMAL (19, 6)
BEGIN
	DECLARE @CountryId NCHAR(2) = dal.fn_Settings__Country();
	DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();

	DECLARE @JoiningDate DATE = dal.fn_Agent__FromDate(@EmployeeId);

	DECLARE @AnnualLeaveRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'AnnualLeave');
	DECLARE @TotalProvisioned DECIMAL (19, 6), @TotalAccruedLeaveDays DECIMAL (19, 6),	@AdditionalDays DECIMAL (19, 6)

	SELECT	@TotalProvisioned = SUM(IIF(E.[Direction] = +1, E.[Quantity], 0))
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	WHERE L.[State] = 4
	AND AC.[Concept] = N'CurrentProvisionsForEmployeeBenefits'
	AND E.[AgentId] = @EmployeeId
	AND E.[ResourceId] = @AnnualLeaveRS;

	SELECT @YearlyAccrual = [Int2] FROM dbo.Agents WHERE [Id] = @EmployeeId;
	SELECT @TotalAccruedLeaveDays = dbo.fn_ActiveDates__AccruedLeaveDays(@JoiningDate, @EndOfServiceDate, @YearlyAccrual, @InactiveDays);

	DECLARE @TotalNonCompensated INT = 0;
	WITH RequestedLeaves AS (
	SELECT Time1, Time2
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.Id = E.LineId
	JOIN dbo.LineDefinitions LD ON LD.Id = L.DefinitionId
	JOIN dbo.Accounts A ON A.Id = E.AccountId
	JOIN dbo.AccountTypes AC ON AC.Id = A.AccountTypeId
	JOIN dbo.Resources R ON R.Id = E.ResourceId
	WHERE AC.[Concept] = N'HRExtension'
	AND LD.[LineType] = 100
	AND R.[Code] = N'AnnualLeave'
	AND L.[State] = 4
	AND L.PostingDate > '2023-10-01'
	AND E.AgentId = @EmployeeId
	),
	ProvisionedLeaves AS ( 
		SELECT E.AgentId, Time1, Time2
		from dbo.Entries E
		JOIN dbo.Lines L ON L.Id = E.LineId
		JOIN dbo.LineDefinitions LD ON LD.Id = L.DefinitionId
		JOIN dbo.Accounts A ON A.Id = E.AccountId
		JOIN dbo.AccountTypes AC ON AC.Id = A.AccountTypeId
		JOIN dbo.Resources R ON R.Id = E.ResourceId
		WHERE AC.[Concept] = N'CurrentProvisionsForEmployeeBenefits'
		AND LD.[LineType] = 100
		AND LD.Code = 'ToProvisionsForEmployeeBenefitsFromEmployeeBenefitsAccruals'
		AND R.[Code] = N'AnnualLeave'
		AND L.[State] = 4
		AND E.AgentId = @EmployeeId
	),
	Compensated AS (
		SELECT DISTINCT RL.Time1, RL.Time2
		FROM RequestedLeaves RL
		JOIN ProvisionedLeaves PL ON PL.Time1 >= RL.Time1 AND PL.Time2 <= RL.Time2
	),
	NonCompensated AS (
	SELECT * FROM RequestedLeaves
	EXCEPT
	SELECT * FROM Compensated
	)
	SELECT @TotalNonCompensated = SUM(DATEDIFF(DAY, [Time1], [Time2]) + 1)
	FROM NonCompensated;

	SELECT @AdditionalDays = ISNULL(@TotalAccruedLeaveDays, 0) - ISNULL(@TotalProvisioned, 0) - ISNULL(@TotalNonCompensated, 0);

	RETURN @AdditionalDays;
END
GO
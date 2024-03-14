CREATE FUNCTION [bll].[fn_Employee_TerminationReason__EOS_SA]
(
	@EmployeeId INT,
	@TerminationBenefitCode NVARCHAR (10)
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE
	@NotedAbsenceDays INT = 20, -- Number of absence days which disrupt service continuity
	@FromDate DATE,
	@ToDate DATE,
	@Result DECIMAL (19, 6);

	SELECT @FromDate = FromDate, @ToDate = ToDate
	FROM dbo.Agents
	WHERE [Id] = @EmployeeId

	-- Service discontinuity is noted if unpaid or breach for 20 days or more (got it from SA law)
	DECLARE @ServiceDaysLost INT = (
		SELECT SUM(E.[Direction] * (DATEDIFF(DAY, E.[Time1], IIF(E.[Time2] <= @ToDate, E.[Time2], @ToDate)) + 1))
		FROM dbo.Entries E
		JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
		JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.[State] = 4
		AND RD.[Code] = N'LeaveTypes'
		AND R.[Code] IN (N'Breach', N'UnpaidLeave')
		AND E.[AgentId] = @EmployeeId
		AND AC.[Concept] = N'HRExtension'
		AND E.[Time1] <= @ToDate
		AND DATEDIFF(DAY, E.[Time1], IIF(E.[Time2] <= @ToDate, E.[Time2], @ToDate)) + 1 >= @NotedAbsenceDays
	);
	IF @ServiceDaysLost IS NOT NULL
		SET @FromDate = DATEADD(DAY, @ServiceDaysLost, @FromDate);

	-- Get service period
	DECLARE @Years INT, @Months INT, @Days INT,
			@Calendar NCHAR (2) = dal.fn_Settings__Calendar();
	SELECT
		@Years = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate), 
		@Months = dbo.fn_FromDate_ToDate__ExtraFullMonths(@Calendar, @FromDate, @ToDate), 
		@Days = dbo.fn_FromDate_ToDate__ExtraFullDays(@Calendar, @FromDate, @ToDate);
	
	-- Get the gross salary		
	DECLARE	@Monthly INT = dal.fn_UnitCode__Id(N'mo');

	DECLARE @GrossSalary DECIMAL (19, 6);
	SET @GrossSalary = (
		SELECT SUM(E.[Direction] * E.[MonetaryValue]) AS [GrossSalary]
		FROM dbo.Entries E
		JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE E.[Time1] <= @ToDate
		AND (E.[Time2] IS NULL OR E.[Time2] >= @ToDate)
		AND L.[State] = 2
		AND LD.[LineType] = 80
		AND AC.[Concept] = N'WagesAndSalarie'
		AND E.[DurationUnitId] = @Monthly AND R.[UnitId] = @Monthly
		GROUP BY E.[CurrencyId]
		HAVING  SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	)
	IF @Years >= 5
	BEGIN
		SET @Result = 0.5 * @GrossSalary * 5;-- print @result
		SET @Result = @Result + @GrossSalary * (@Years - 5 + @Months / 12.0 + @Days / 360.0); --print @result
	END
	ELSE
		SET @Result = 0.5 * @GrossSalary * (@Years + @Months / 12.0 + @Days / 360.0); --print @result

	SET @Result = CASE 
		WHEN @TerminationBenefitCode = N'TB0' THEN 0
		WHEN @TerminationBenefitCode = N'TB2' THEN @Result
		WHEN @TerminationBenefitCode = N'TB4' THEN CASE
			WHEN  @Years < 2 THEN 0
			WHEN @Years < 5 THEN @Result / 3		
			WHEN @Years < 10 THEN @Result * 2 / 3
			ELSE @Result
		END
	END;

	RETURN ROUND(@Result, 2)
END
GO
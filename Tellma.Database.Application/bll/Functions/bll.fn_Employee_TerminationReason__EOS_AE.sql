CREATE FUNCTION [bll].[fn_Employee_TerminationReason__EOS_AE]
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
	
	-- No gratuity if service period is less than a full year.
	IF @Years = 0 RETURN 0;

	-- Get the basic salary		
	DECLARE @EmployeesDates dbo.IdDateList;
	INSERT INTO @EmployeesDates VALUES (@EmployeeId, @ToDate);
	DECLARE @BasicSalary DECIMAL (19, 6) = (
		SELECT SUM([BasicSalary])
		FROM dal.ft_EmployeesDates__EmployeesProfiles (@EmployeesDates)
	);
	-- Get if Expat
	DECLARE @CitizenshipId INT = (SELECT Lookup2Id FROM dbo.Agents WHERE [Id] = @EmployeeId);
	DECLARE @CitizenshipCode NCHAR (3) = dal.fn_Lookup__Code(@CitizenshipId);
	DECLARE @IsDomestic BIT = IIF(@CitizenshipCode = N'ARE', 1, 0);

	IF @IsDomestic = 1
	BEGIN
		SET @Result = 14.0 / 30.0 * @BasicSalary * (@Years + @Months / 12.0 + @Days / 360.0); --print @result
		RETURN CASE
			-- No gratuity for domestic worker, if resigned without reason or breached the contract.
			WHEN @TerminationBenefitCode = N'DL0' THEN 0
			-- Else 2 weeks for every year of service
			WHEN @TerminationBenefitCode = N'DL1' THEN @Result
			-- ELse Return a negative value to cause an issue at the UI and raise a red flag
			ELSE -1 -- Wrong choice for domestic employee
		END
	END

	-- For Expats
	-- For all cases, except expat employee resigning from unlimited contract
	IF @TerminationBenefitCode = N'EL2'
	BEGIN
		IF @Years >= 5
		BEGIN
			-- 3 weeks per year for the first 5 years
			SET @Result = 21.0/30.0 * @BasicSalary * 5;-- print @result
			-- add to it one month per year for the additional period
			SET @Result = @Result + @BasicSalary * (@Years - 5 + @Months / 12.0 + @Days / 360.0); --print @result
		END
		ELSE
			SET @Result = 21.0/30.0 * @BasicSalary * (@Years + @Months / 12.0 + @Days / 360.0); --print @result
	END
	-- Expat employee resigning from unlimited contract, only applicable for expats hired before Feb 2022
	ELSE IF @TerminationBenefitCode = N'EU3'
	BEGIN
		IF (@Years >= 5)
		BEGIN
			-- 1 week per year for the first 3 years, then 2 weeks per year for the next 5 years
			SET @Result = 7.0 / 30.0 * @BasicSalary * 3 + 14.0 / 30.0 * @BasicSalary * 2;-- print @result
			-- add to it 3 weeks per year for the additional period
			SET @Result = @Result + 21.0 / 30.0 * @BasicSalary * (@Years - 5 + @Months / 12.0 + @Days / 360.0); --print @result
		END
		ELSE IF (@Years >=3)
		BEGIN
			-- 1 week per year for the first 3 years, then 2 weeks per year for the next 3 years
			SET @Result = 7.0 / 30.0 * @BasicSalary * 2
			-- add to it one month per year for the additional period
			SET @Result = @Result + 14.0 / 30.0 * @BasicSalary * (@Years - 3 + @Months / 12.0 + @Days / 360.0); --print @result
		END
		ELSE
			SET @Result = 7.0 / 30.0 * @BasicSalary * (@Years + @Months / 12.0 + @Days / 360.0); --print @result
	END
	ELSE
		SET @Result = -2; -- wrong choice for expat employee

	RETURN ROUND(@Result, 2)
END
GO
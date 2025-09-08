CREATE FUNCTION bll.ft_AnnualLeaveSalaryAdvances(
	@LastIndex INT,
	@EmployeeId INT,
	@Time1 DATE,
	@Time2 DATE,
	@DeductLoans BIT = 1
)
-- @Time1 < @Time2 and fall in the same month
RETURNS 
--DECLARE 
@Shortlines TABLE
(
	[Index]			INT	,
	[CurrencyId0]	NCHAR (3),
	[MonetaryValue0]DECIMAL (19, 6),
	[NotedDate0]	DATE,
	[AgentId0]		INT,
	[Memo]			NVARCHAR (255),
	[Time10]		DATE,
	[Time20]		DATE
)
AS
BEGIN
-- Use same logic to calculate salaries, except we are recording it as loan, so no resource involved
	DECLARE @ContractLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccruals.M');
	DECLARE @ContractAmendmentLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsAmended.M');
	DECLARE @ContractTerminationLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M');

	DECLARE @DeductionLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitAccrualsFromTradePayables.M');
	DECLARE @DeductionAmendmentLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitAccrualsFromTradePayablesAmended.M');
	DECLARE @DeductionTerminationLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitAccrualsFromTradePayablesTerminated.M');

	DECLARE @SSDeductionLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToSSContributionsAndAccrualsFromSSPayables.M');
	DECLARE @SSDeductionAmendmentLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToSSContributionsAndAccrualsFromSSPayablesAmended.M');
	DECLARE @SSDeductionTerminationLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToSSContributionsAndAccrualsFromSSPayablesTerminated.M');

	DECLARE
	@DurationUnitId INT = dal.fn_UnitCode__Id(N'mo');
	DECLARE
	@StartOfMonth DATE = [dbo].[fn_MonthStart](@Time1),
	@EndOfMonth DATE = [dbo].[fn_MonthEnd](@Time1),
	@WorkDays INT = DATEDIFF(DAY, @Time1, @Time2) + 1;
	DECLARE
	@MonthDays INT = DATEDIFF(DAY, @StartOfMonth, @EndOfMonth) + 1,
	@WagesAndSalariesMemo NVARCHAR(255) = N'مرتبات ', 
	@AnnualLeavesMemo NVARCHAR(255) = N'مرتب الإجازة - ',
	@DeductionsMemo NVARCHAR (255) = N'خصومات ', 
	@SSDeductionsMemo NVARCHAR (255) = N'استقطاع ', 
	@LoanDeductionsMemo NVARCHAR (255) = N'استقطاع سلف ',
	@AbdenceDeductionsMemo NVARCHAR (255) = N'استقطاع ',
	-- better make the memo like: Salaries of the period 5/1 - 17/2, 2023 مرتبات 1 - 31 أغسطس
	@MonthName NVARCHAR (50) = FORMAT(@Time1, 'MMMM', 'ar-AE') + N' ' + CAST(YEAR(@Time1) AS NVARCHAR(50)) + N': ',
	@WagesAndSalariesEntryTypeId INT = dal.fn_EntryTypeConcept__Id(N'WagesAndSalaries');

	WITH EffectiveWorkSalary AS (
		SELECT S.[NotedAgentId0], S.[CurrencyId1],  S.[Time10], S.[Time20],-- S.[NotedResourceId0], S.[AgentId0], S.[CenterId0],
			SUM(S.[MonetaryValue1]) AS [MonetaryValue1]
		FROM bll.ft_Widelines_Period_EventFromModel__Generate( -- Important. We use PIT version, because we don't want prorating
			@ContractLineDefinitionId,
			@ContractAmendmentLineDefinitionId,
			@ContractTerminationLineDefinitionId,
			@Time1,
			@Time2,
			@DurationUnitId,
			0,			-- @EntryIndex
			NULL,			-- @AgentId
			NULL,--@BasicSalary,		-- @EmployeeBenefitId,	-- @ResourceId
			@EmployeeId, 	-- @EmployeeId,		-- @NotedAgentId
			NULL,			-- @JobId,			-- @NotedResourceId
			NULL			--@CenterId
		) S
		JOIN dbo.Resources R ON R.[Id] = S.[ResourceId0]
		WHERE R.UnitId = @DurationUnitId -- Excludes Overtime
		AND S.[EntryTypeId1] = @WagesAndSalariesEntryTypeId
		GROUP BY S.[NotedAgentId0], S.[CurrencyId1], S.[Time10], S.[Time20]--, S.[NotedResourceId0], S.[AgentId0], S.[CenterId0]
	)
	INSERT INTO @Shortlines([Index],	[CurrencyId0], [MonetaryValue0], [NotedDate0], [AgentId0], [Time10], [Time20], [Memo])
	SELECT ROW_NUMBER() OVER (ORDER BY [NotedAgentId0], [CurrencyId1], [Time10]) + @LastIndex,
		[CurrencyId1], [MonetaryValue1] * (DATEDIFF(DAY, Time10, Time20) + 1) / @MonthDays, @EndOfMonth, [NotedAgentId0], [Time10], [Time20],
		@WagesAndSalariesMemo + @MonthName + CAST(DAY(Time10) AS NVARCHAR (5)) + N' - ' + CAST(DAY(Time20) AS NVARCHAR(5))
	FROM EffectiveWorkSalary;
	SELECT @LastIndex = ISNULL(MAX([Index]), -1) FROM @Shortlines;

	WITH EffectiveDeductions AS (
		SELECT [AgentId0], [CurrencyId0], [Time10], [Time20], SUM([MonetaryValue0]) AS [MonetaryValue0]
		FROM bll.ft_Widelines_Period_EventFromModel__Generate( -- Important. We use PIT version, because we don't want prorating
			@DeductionLineDefinitionId,
			@DeductionAmendmentLineDefinitionId,
			@DeductionTerminationLineDefinitionId,
			@Time1,
			@Time2,
			@DurationUnitId,
			1,			-- @EntryIndex
			NULL,			-- @AgentId
			NULL,--@BasicSalary,		-- @EmployeeBenefitId,	-- @ResourceId
			@EmployeeId, 	-- @EmployeeId,		-- @NotedAgentId
			NULL,			-- @JobId,			-- @NotedResourceId
			NULL			--@CenterId
		)
		GROUP BY [AgentId0], [CurrencyId0], [Time10], [Time20]
	)
	INSERT INTO @Shortlines([Index],	[CurrencyId0], [MonetaryValue0], [NotedDate0], [AgentId0], [Time10], [Time20], [Memo])
		SELECT 1 + @LastIndex, [CurrencyId0], - [MonetaryValue0] * (DATEDIFF(DAY, Time10, Time20) + 1) / @MonthDays, @EndOfMonth, [AgentId0], [Time10], [Time20],
		@DeductionsMemo + @MonthName + CAST(DAY(Time10) AS NVARCHAR (5)) + N' - ' + CAST(DAY(Time20) AS NVARCHAR(5))
	FROM EffectiveDeductions;
	SELECT @LastIndex = MAX([Index]) FROM @Shortlines;

	WITH EffectiveSSDeductions AS (
		SELECT [NotedAgentId0], [AgentId2], [CurrencyId0], [Time10], [Time20], SUM([MonetaryValue1]) AS [MonetaryValue0]
		FROM bll.ft_Widelines_Period_EventFromModel__Generate( -- Important. We use PIT version, because we don't want prorating
			@SSDeductionLineDefinitionId,
			@SSDeductionAmendmentLineDefinitionId,
			@SSDeductionTerminationLineDefinitionId,
			@Time1,
			@Time2,
			@DurationUnitId,
			0,			-- @EntryIndex
			NULL,			-- @AgentId
			NULL,--@BasicSalary,		-- @EmployeeBenefitId,	-- @ResourceId
			@EmployeeId, 	-- @EmployeeId,		-- @NotedAgentId
			NULL,			-- @JobId,			-- @NotedResourceId
			NULL			--@CenterId
		)
		GROUP BY [NotedAgentId0], [AgentId2], [CurrencyId0], [Time10], [Time20]
	)
	INSERT INTO @Shortlines([Index], [CurrencyId0], [MonetaryValue0], [NotedDate0], [AgentId0], [Time10], [Time20], [Memo])
	-- I think we don't need to prorate here, as the event from model does it automatically
	SELECT-- 1 + @LastIndex, 
		ROW_NUMBER() OVER (ORDER BY [NotedAgentId0], [CurrencyId0], [Time10]) + @LastIndex,
	[CurrencyId0], - [MonetaryValue0] * (DATEDIFF(DAY, Time10, Time20) + 1) / @MonthDays, @EndOfMonth, [NotedAgentId0],[Time10], [Time20],
			@SSDeductionsMemo + N' ' + dal.fn_Agent__Name2([AgentId2]) + N' ' + @MonthName + CAST(DAY(Time10) AS NVARCHAR (5)) + N' - ' + CAST(DAY(Time20) AS NVARCHAR(5))
	FROM EffectiveSSDeductions;
	SELECT @LastIndex = MAX([Index]) FROM @Shortlines;

	IF @DeductLoans = 1
	WITH LoanDeductions AS (
		SELECT E.[AgentId] AS [AgentId0], E.[CurrencyId] AS [CurrencyId0], SUM([Direction] * [MonetaryValue]) AS [MonetaryValue0]
		FROM dbo.Entries E
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE AC.[Concept] = N'CurrentFinancialAssetsAtAmortisedCost'
		AND ET.[Concept] IN (N'IncreaseDecreaseThroughTransfersFinancialAssets', N'IncreaseThroughOriginationOrPurchaseFinancialAssets')
		AND L.[State] = 4
		AND E.[NotedDate] = @EndOfMonth
		AND (@EmployeeId IS NULL OR E.[AgentId] = @EmployeeId)
		GROUP BY E.[AgentId], E.[CurrencyId]
	)
	INSERT INTO @Shortlines([Index],	[CurrencyId0], [MonetaryValue0], [NotedDate0], [AgentId0], [Time10], [Time20], [Memo])
		--I believe we need to do it here
		SELECT 1 + @LastIndex, [CurrencyId0], - [MonetaryValue0] * @WorkDays/@MonthDays, @EndOfMonth, [AgentId0], @Time1, @Time2, 
		@LoanDeductionsMemo + @MonthName+ CAST(DAY(@Time1) AS NVARCHAR (5)) + N' - ' + CAST(DAY(@Time2) AS NVARCHAR(5))
	FROM LoanDeductions;
	SELECT @LastIndex = MAX([Index]) FROM @Shortlines;
	
DECLARE @HRExtenstion INT = dal.fn_AccountTypeConcept__Id(N'HRExtension');
DECLARE @LeaveTypesRD INT = dal.fn_ResourceDefinitionCode__Id(N'LeaveTypes');

-- TODO: Add special handling for absences with upper limits, such as Sick Leave, etc
WITH UnpaidAbsenceLogs AS (
	SELECT E.AgentId AS [EmployeeId], E.[ResourceId],
		IIF(E.[Time1] < @Time1, @Time1, E.[Time1]) AS FromDate,
		IIF(E.[Time2] > @Time2, @Time2, E.[Time2]) AS ToDate
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	WHERE L.[State] = 4
	AND (@EmployeeId IS NULL OR E.[AgentId] = @EmployeeId)
	AND A.[AccountTypeId] = @HRExtenstion
	AND R.[DefinitionId] = @LeaveTypesRD
	AND E.[Time1] <= @Time2
	AND ISNULL(E.[Time2], N'9999-12-31') >= @Time1
	AND R.[Code] IN (N'UnpaidLeave') -- excluded annual leave
) --select * from UnpaidAbsenceLogs
INSERT INTO @Shortlines([Index],	[CurrencyId0], [MonetaryValue0], [NotedDate0], [AgentId0], [Time10], [Time20], [Memo])
SELECT ROW_NUMBER() OVER(ORDER BY AL.[FromDate]) + @LastIndex AS [Index], [CurrencyId1],
	-SUM([MonetaryValue1] * (DATEDIFF(DAY, SS.[Time10], SS.[Time20]) + 1)) / @MonthDays AS [MonetaryValue0],
	@EndOfMonth, [NotedAgentId0], AL.[FromDate], AL.[ToDate],
	@AbdenceDeductionsMemo + dal.fn_Resource__Name2(AL.[ResourceId]) + @MonthName + CAST(DAY(Time10) AS NVARCHAR (5)) + N' - ' + CAST(DAY(Time20) AS NVARCHAR(5))
FROM UnpaidAbsenceLogs AL
CROSS APPLY [bll].[ft_Widelines_Period_EventFromModel__Generate](
	@ContractLineDefinitionId,
	@ContractAmendmentLineDefinitionId,
	@ContractTerminationLineDefinitionId,
	AL.[FromDate],
	AL.[ToDate],
	@DurationUnitId,
	0,			-- @EntryIndex
	NULL,			-- @AgentId
	NULL, -- @EmployeeBenefitId,	-- @ResourceId
	AL.[EmployeeId],		-- @NotedAgentId
	NULL, --@JobId,			-- @NotedResourceId
	NULL --@CenterId);
)  SS
JOIN dbo.Resources R ON R.[Id] = SS.[ResourceId0]
WHERE R.[UnitId] = @DurationUnitId
AND SS.[EntryTypeId1] = @WagesAndSalariesEntryTypeId
GROUP BY
	AL.[FromDate], AL.[ToDate], [CurrencyId1], [NotedAgentId0], AL.[ResourceId], SS.[Time10], SS.[Time20];

	RETURN
END
GO
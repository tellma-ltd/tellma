CREATE PROCEDURE [wiz].[EmployeeBenefits__TaxAndDeduct]
@EmployeeId INT,
@TaxDepartmentId INT,
@CenterId INT,
@Documents DocumentList READONLY,
@Lines LineList READONLY,
@Entries EntryList READONLY
AS
DECLARE @Country NCHAR (2) = dal.[fn_Settings__Country]();
DECLARE @ContractLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitAccrualsFromTradePayables.M');
DECLARE @ContractAmendmentLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitAccrualsFromTradePayablesAmended.M');
DECLARE @ContractTerminationLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitAccrualsFromTradePayablesTerminated.M');

DECLARE @Monthly INT = dal.fn_UnitCode__Id(N'mo');
DECLARE @PostingDate DATE = (SELECT TOP 1 [PostingDate] FROM @Documents);

DECLARE @PeriodStart DATE = dbo.fn_MonthStart(@PostingDate);
DECLARE @PeriodEnd DATE = dbo.fn_MonthEnd(@PostingDate);
DECLARE @PeriodLength INT = DATEDIFF(DAY, @PeriodStart, @PeriodEnd) + 1;

DECLARE @WagesAndSalariesNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'WagesAndSalaries');
DECLARE @Widelines WidelineList;
INSERT INTO @Widelines
SELECT * FROM bll.ft_Widelines_Period_EventFromModel__Generate(
	@ContractLineDefinitionId,
	@ContractAmendmentLineDefinitionId,
	@ContractTerminationLineDefinitionId,
	@PeriodStart, @PeriodEnd,
	@Monthly,
	1,			-- @EntryIndex
	@TaxDepartmentId,	-- @AgentId
	NULL,			-- @ResourceId
	@EmployeeId,		-- @NotedAgentId
	NULL,			-- @NotedResourceId
	@CenterId)
WHERE [Value1] IS NOT NULL; -- TODO: Investigate, why do we need this condition?

DECLARE @PeriodBenefits  [dbo].[PeriodBenefitsList];

WITH PeriodBenefitEntries AS (
	SELECT E.[NotedAgentId], R.[Code] AS ResourceCode, E.[Direction] * E.[Value] AS [Value]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.[Resources] R ON R.[Id] = E.[ResourceId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	WHERE L.[State] = 4 AND L.[DocumentId] NOT IN (SELECT [Id] FROM @Documents)
	AND AC.[Node].IsDescendantOf(@WagesAndSalariesNode) = 1
	AND L.[PostingDate] BETWEEN @PeriodStart AND @PeriodEnd
	AND (@EmployeeId IS NULL OR E.[NotedAgentId] = @EmployeeId)
	UNION ALL
	SELECT E.[NotedAgentId], R.[Code] AS ResourceCode,
		bll.fn_ConvertToFunctional(@PeriodEnd, E.[CurrencyId], E.[Direction] * E.[MonetaryValue]) AS [Value]
	FROM @Entries E
	JOIN dbo.[Resources] R ON R.[Id] = E.[ResourceId]
	JOIN @Lines L ON L.[Index] = E.[LineIndex] AND L.[DocumentIndex] = E.[DocumentIndex]
--	Before saving, accounts are not detected yet, so we need to rely on resource definition.
	JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
	WHERE RD.[Code] = N'EmployeeBenefits'
	AND (@EmployeeId IS NULL OR E.[NotedAgentId] = @EmployeeId)
) -- In SD, foreign currency benefits are translated to local currency for deductions
INSERT INTO @PeriodBenefits([Id], [EmployeeId], [ResourceCode], --[CurrencyId], [MonetaryValue], 
[Value]) 
SELECT ROW_NUMBER() OVER(ORDER BY [NotedAgentId], [ResourceCode]) - 1 AS [Id], [NotedAgentId] AS [EmployeeId], [ResourceCode], --NULL AS [CurrencyId], NULL AS [MonetaryValue],
	SUM([Value]) AS [Value]
FROM PeriodBenefitEntries
GROUP BY [NotedAgentId], [ResourceCode];
--select * from bll.ft_Employees__Deductions(@Country, @PeriodBenefits, @PeriodStart, @PeriodEnd)
UPDATE WL
SET 
	[CurrencyId1] 	= SS.[CurrencyId],
	[MonetaryValue1] = SS.[MonetaryValue],
	[NotedAmount1] = 100--SS.[NotedAmount]
FROM @Widelines WL
CROSS APPLY bll.ft_Employees__Deductions(@Country, @PeriodBenefits, @PeriodStart, @PeriodEnd) SS
WHERE WL.[NotedAgentId1] = SS.[EmployeeId] AND WL.[AgentId1] = SS.[DeductionAgentId];
DELETE @Widelines WHERE [MonetaryValue1] = 0;
--select * from @@Widelines

DECLARE @EmployeeIncomeTaxAG INT = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'EmployeeIncomeTax');
WITH WideLinesSorted AS (
	SELECT [Index], ROW_NUMBER() OVER (ORDER BY dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]), [Index]) - 1 AS [DefragmentedIndex]
	FROM @Widelines WL
	JOIN dbo.Agents AG ON AG.[Id] = WL.[NotedAgentId1]
)
UPDATE WL
SET
	[Index] = WLS.[DefragmentedIndex],
--	MA: 2024-07-05. The following line was causing the income tax to be further reduced for employees joining or leaving during the month in ET
--	[MonetaryValue1] = ROUND([MonetaryValue1] * (DATEDIFF(DAY, [Time10], [Time20]) + 1.0) / @PeriodLength, 2)
	[MonetaryValue1] = CASE
		WHEN [AgentId1] = @EmployeeIncomeTaxAG THEN ROUND([MonetaryValue1], 2) -- For EIT, leave it as is.
		ELSE ROUND([MonetaryValue1] * (DATEDIFF(DAY, [Time10], [Time20]) + 1.0) / @PeriodLength, 2) -- For SS in old design, such as in 302, prorate.
	END
FROM @Widelines WL
JOIN WideLinesSorted  WLS ON WLS.[Index] = WL.[Index];

-- For income tax, we do not split the tax over multiple centers. We assign to the last center
IF EXISTS(SELECT * FROM @Widelines WHERE [AgentId1] = @EmployeeIncomeTaxAG)
BEGIN
	DECLARE @BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');
	WITH RankedCenters AS (
		SELECT
			T.[NotedAgentId],
			T.[CenterId],
			T.[Time1],
			LEAD(T.[Time1]) OVER (PARTITION BY T.[NotedAgentId] ORDER BY T.[Time1] DESC) AS NextJoiningDate
		FROM @Entries T
		WHERE ResourceId = @BasicSalaryRS
	),
	EmployeesCenters AS (
		SELECT [NotedAgentId], [CenterId]
		FROM RankedCenters
		WHERE NextJoiningDate IS NULL
	)
	UPDATE WL
	SET [CenterId0] = EC.[CenterId]
	FROM @Widelines WL
	JOIN EmployeesCenters EC ON EC.[NotedAgentId] = WL.[NotedAgentId1]
	WHERE WL.[AgentId1] = @EmployeeIncomeTaxAG
END
SELECT * FROM @Widelines;
GO
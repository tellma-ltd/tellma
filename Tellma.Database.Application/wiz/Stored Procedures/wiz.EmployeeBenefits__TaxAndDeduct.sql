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

DECLARE @DurationUnitId INT = dal.fn_UnitCode__Id(N'mo');
DECLARE @PostingDate DATE = (SELECT TOP 1 [PostingDate] FROM @Documents);
DECLARE @PeriodEnd DATE = [dbo].[fn_PeriodEnd](@DurationUnitId, @PostingDate);
DECLARE @PeriodStart DATE = [dbo].[fn_PeriodStart](@DurationUnitId, @PostingDate);
DECLARE @PeriodLength INT = DATEDIFF(DAY, @PeriodStart, @PeriodEnd) + 1;

DECLARE @WagesAndSalariesNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'WagesAndSalaries');
DECLARE @Widelines WidelineList;
INSERT INTO @Widelines
SELECT * FROM bll.ft_Widelines_Period_EventFromModel__Generate(
	@ContractLineDefinitionId,
	@ContractAmendmentLineDefinitionId,
	@ContractTerminationLineDefinitionId,
	@PeriodStart, @PeriodEnd,
	@DurationUnitId,
	1,			-- @EntryIndex
	@TaxDepartmentId,	-- @AgentId
	NULL,			-- @ResourceId
	@EmployeeId,		-- @NotedAgentId
	NULL,			-- @NotedResourceId
	@CenterId)
WHERE [Value1] IS NOT NULL; -- TODO: Investigate, why do we need this condition?

DECLARE @PeriodBenefits  [dbo].[PeriodBenefitsList];

WITH PeriodBenefitEntries AS (
	SELECT E.[NotedAgentId], R.[Code] AS ResourceCode, --E.[CurrencyId], E.[Direction] * E.[MonetaryValue] AS [MonetaryValue], 
	E.[Direction] * E.[Value] AS [Value]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.[Resources] R ON R.[Id] = E.[ResourceId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	WHERE L.[State] = 4 AND L.[DocumentId] NOT IN (SELECT [Id] FROM @Documents)
	AND AC.[Node].IsDescendantOf(@WagesAndSalariesNode) = 1
	AND L.[PostingDate] BETWEEN @PeriodStart AND @PeriodEnd
	AND (@EmployeeId IS NULL OR E.[NotedAgentId] = @EmployeeId)
	UNION
	SELECT E.[NotedAgentId], R.[Code] AS ResourceCode, --E.[CurrencyId], E.[Direction] * E.[MonetaryValue] AS [MonetaryValue],
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
--declare @count int = (select count(*) from @PeriodBenefits);
--declare @msg nvarchar(255) = N'count = ' + cast(@count as nvarchar(255))
--raiserror(@msg, 16, 1);

UPDATE WL
SET 	[CurrencyId1] 	= SS.[CurrencyId],
	[MonetaryValue1] = SS.[MonetaryValue]
FROM @Widelines WL
CROSS APPLY bll.ft_Employees__Deductions(@Country, @PeriodBenefits, @PeriodStart, @PeriodEnd) SS
WHERE WL.[NotedAgentId1] = SS.[EmployeeId] AND WL.[AgentId1] = SS.[DeductionAgentId];
DELETE @Widelines WHERE [MonetaryValue1] = 0;
--select * from @@Widelines
WITH WideLinesSorted AS (
	SELECT [Index], ROW_NUMBER() OVER (ORDER BY dbo.fn_Localize(AG.[Name], AG.[Name2], AG.[Name3]), [Index]) - 1 AS [DefragmentedIndex]
	FROM @Widelines WL
	JOIN dbo.Agents AG ON AG.[Id] = WL.[NotedAgentId1]
)
UPDATE WL
SET
	[Index] = WLS.[DefragmentedIndex],
	[MonetaryValue1] = ROUND([MonetaryValue1] * (DATEDIFF(DAY, [Time10], [Time20]) + 1.0) / @PeriodLength, 2)
FROM @Widelines WL
JOIN WideLinesSorted  WLS ON WLS.[Index] = WL.[Index];

SELECT * FROM @Widelines;
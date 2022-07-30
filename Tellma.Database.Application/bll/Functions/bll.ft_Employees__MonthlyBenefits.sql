CREATE FUNCTION [bll].[ft_Employees__MonthlyBenefits](
	@EmployeeIds dbo.IdList READONLY,
	@LineIds dbo.IdList READONLY,
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[EmployeeId] INT,
	[ResourceCode] NVARCHAR (50),
	[CurrencyId] NCHAR (3),
	[MonetaryValue] DECIMAL (19, 6),
	[Value] DECIMAL (19, 6)
)
AS BEGIN
	DECLARE @DocumentIds IdList; INSERT INTO @DocumentIds SELECT DISTINCT [DocumentId] FROM dbo.Lines WHERE [Id] IN (SELECT [Id] FROM @LineIds);
	DECLARE @BenefitLineIds IdList; INSERT INTO @BenefitLineIds SELECT [Id] FROM dbo.Lines WHERE [State] >= 0 AND [DocumentId] IN (SELECT [Id] FROM @DocumentIds);
	
	INSERT INTO @MyResult
	SELECT E.[NotedAgentId] AS [EmployeeId], R.[Code] AS [ResourceCode], E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS [MonetaryValue],
		bll.fn_ConvertToFunctional(@PeriodEnd, E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue])) AS [Value]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
	WHERE RD.[Code] = N'EmployeeBenefits'
	AND R.[UnitId] = dal.fn_UnitCode__Id(N'mo')
	AND E.[Time1] >= @PeriodStart AND E.[Time2] <= @PeriodEnd 
	AND (E.[NotedAgentId] IN (SELECT [Id] FROM @EmployeeIds))
	AND (L.[State] = 4 OR L.[Id] IN (SELECT [Id] FROM @BenefitLineIds))
	GROUP BY E.[NotedAgentId], R.[Code], E.[CurrencyId]
	RETURN
END
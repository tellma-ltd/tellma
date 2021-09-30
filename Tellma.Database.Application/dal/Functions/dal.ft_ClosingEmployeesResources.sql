CREATE FUNCTION [dal].[ft_ClosingEmployeesResources] (
	@DefinitionId INT,
	@MonthEnding DATE
)
RETURNS @returntable TABLE
(
	[EmployeeId]	INT,
	[AsOf]			DATE,
	[ResourceId]	INT,
	[ResourceCode]	NVARCHAR(10),
	[Lookup1Code]	NVARCHAR(10)
)
AS
BEGIN
	INSERT @returntable
	SELECT E.[AgentId], E.[Time1], E.[ResourceId], R.[Code] AS [ResourceCode], LKP.[Code] AS Lookup1Code
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	LEFT JOIN dbo.Lookups LKP ON LKP.[Id] = R.[Lookup1Id]
	WHERE AC.[Concept] = N'HRExtension'
	AND L.[State] = 2
	AND E.[Time1] <= @MonthEnding
	AND R.[DefinitionId] = @DefinitionId
	GROUP BY E.[AgentId], E.[Time1], E.[ResourceId], R.[Code], LKP.[Code]
	HAVING SUM([Direction] * [Quantity]) <> 0 
	RETURN
END
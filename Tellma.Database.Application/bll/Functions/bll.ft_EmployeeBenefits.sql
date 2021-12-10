CREATE FUNCTION [bll].[ft_EmployeeBenefits]
(
	@EmployeesBenefitsDefinitionCode NVARCHAR (50),
	@BenefitCLassCode NVARCHAR (50), -- Always Lookup 1
	@PeriodEnd DATE,
	@EmployeeId INT
)
RETURNS @returntable TABLE
(
	[AgentId] INT,
	[CenterId] INT,
	[NetValue] DECIMAL (19, 4)
)
AS
BEGIN
	INSERT @returntable
	SELECT E.[AgentId], E.[CenterId], SUM([Direction] * [Value]) AS NetValue
	FROM dbo.Documents D
	JOIN dbo.DocumentDefinitions DD ON DD.[Id] = D.[DefinitionId]
	JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
	JOIN dbo.Lookups LK ON LK.[Id] = R.[Lookup1Id]
	WHERE DD.DocumentType = 0
	AND L.[State] = 2
	AND AC.Concept = N'WagesAndSalaries'
	AND RD.[Code] = @EmployeesBenefitsDefinitionCode
	AND LK.[Code] = @BenefitCLassCode
	AND E.[NotedDate] = @PeriodEnd
	AND (@EmployeeId IS NULL OR E.AgentId = @EmployeeId)
	GROUP BY E.[AgentId], E.[CenterId]
	HAVING SUM([Direction] * [Value]) <> 0
	RETURN
END
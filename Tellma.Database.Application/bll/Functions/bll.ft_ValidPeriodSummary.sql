CREATE FUNCTION [bll].[ft_ValidPeriodSummary]
(
	@AccountTypeConcept NVARCHAR (255),
	@AgentDefinitionCode NVARCHAR (255),
	@ResourceDefinitionCode NVARCHAR (255),
	@NotedResourceDefinitionCode NVARCHAR (255),
	@LineType TINYINT, -- 20: T for P, 40: Plan, 60:T for T, 80:Template, 100: Event, 120: Regulatory
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[CenterId] INT,
	[AgentId]	INT,
	[AccountId] INT,
	[CurrencyId] NCHAR (3),
	[ResourceId] INT,
	[DurationUnitId] INT,
	[NotedResourceId] INT,
	[ValidFrom] DATE,
	[ValidTill] Date,
	[Quantity] DECIMAL (19, 10),
	[MonetaryValue] DECIMAL (19,10),
	[EmployeeId] INT,
	[CustomerId] INT,
	[SupplierId] INT
)
AS
BEGIN
	DECLARE @State TINYINT = IIF(@LineType < 100, 2, 4);
	DECLARE @AccountTypeNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = @AccountTypeConcept);
	DECLARE @AgentDefinitionId INT = (SELECT [Id] FROM dbo.AgentDefinitions WHERE [Code] = @AgentDefinitionCode);
	DECLARE @ResourceDefinitionId INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = @ResourceDefinitionCode);
	DECLARE @NotedResourceDefinitionId INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = @NotedResourceDefinitionCode);

	DECLARE  @T TABLE (
		[VId] INT PRIMARY KEY,
		[VTime1] DATE,
		[VTime2] DATE,
		[NextTime] DATE,
		[ValidFrom] DATE,
		[ValidTill] Date,
		[CenterId] INT,
		[AgentId] INT, 
		[AccountId] INT,
		[CurrencyId] NCHAR (3),
		[ResourceId] INT,
		[DurationUnitId] INT,
		[NotedResourceId] INT,
		[Quantity] DECIMAL (19, 6),
		[MonetaryValue] DECIMAL (19,6),
		[EmployeeId] INT,
		[CustomerId] INT,
		[SupplierId] INT
	)
	INSERT INTO @T([VId], [VTime1], [NextTime], [VTime2], [CenterId], [AgentId], [AccountId], [CurrencyId], [ResourceId], [Quantity],
		[DurationUnitId], [NotedResourceId], [MonetaryValue], [EmployeeId], [CustomerId], [SupplierId])
	SELECT
		E.[Id],
		E.[Time1] AS VTime1,
		LEAD(E.[Time1], 1, N'9999.12.31') OVER (
			PARTITION BY E.[AccountId], E.[AgentId], E.[ResourceId], L.[EmployeeId], L.[CustomerId], L.[SupplierId]
			ORDER BY L.[EmployeeId], E.[ResourceId], E.[Time1]
		) As [NextTime],
		ISNULL(E.[Time2], N'9999.12.31') AS VTime2,
		E.[CenterId], E.[AgentId], E.[AccountId], E.[CurrencyId], E.[ResourceId], E.[Quantity], E.[DurationUnitId], E.[NotedResourceId], E.[MonetaryValue],
		L.[EmployeeId], L.[CustomerId], L.[SupplierId]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.AccountTypeId
	LEFT JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	LEFT JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	LEFT JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
	WHERE LD.[LineType] = @LineType
	AND L.[State] = @State
	AND (AC.[Node].IsDescendantOf(@AccountTypeNode) = 1)
/* MA: Commented Fab 13m as wrong presence of Employee in Agent Column was causing the account to b excluded
	AND (AG.Id IS NULL AND @AgentDefinitionId IS NULL
		OR AG.[DefinitionId] = @AgentDefinitionId) */
	AND (@AgentDefinitionId IS NULL	OR AG.[DefinitionId] = @AgentDefinitionId)
	AND (R.[Id] IS NULL AND @ResourceDefinitionId IS NULL --deductions have null resource
		OR R.[DefinitionId] = @ResourceDefinitionId)
	AND (NR.[Id] IS NULL AND @NotedResourceDefinitionId IS NULL
		OR NR.[DefinitionId] = @NotedResourceDefinitionId);

	UPDATE @T
	SET 
		[ValidTill] = IIF ([NextTime] < [VTime2], DATEADD(DAY,-1,[NextTime]), [VTime2])

	UPDATE @T
	SET 
		[ValidFrom] = IIF (@PeriodStart > 	[VTime1], @PeriodStart, [VTime1]),
		[ValidTill] = IIF (@PeriodEnd < 	[ValidTill], @PeriodEnd, [ValidTill])

	UPDATE @T
	SET [Quantity] = ISNULL([Quantity], 1) * (1 + DATEDIFF(DAY, [ValidFrom], [ValidTill])) / (1 + DATEDIFF(DAY,@PeriodStart, @PeriodEnd));
	UPDATE @T SET [MonetaryValue] = [Quantity] * [MonetaryValue]

	INSERT INTO @MyResult([CenterId], [AgentId], [AccountId], [CurrencyId], [ResourceId], [DurationUnitId], [NotedResourceId], [ValidFrom],	[ValidTill],[Quantity], [MonetaryValue], [EmployeeId], [CustomerId], [SupplierId])
	SELECT [CenterId], [AgentId], [AccountId], [CurrencyId], [ResourceId], [DurationUnitId], [NotedResourceId], [ValidFrom], [ValidTill],[Quantity], [MonetaryValue], [EmployeeId], [CustomerId], [SupplierId]
	FROM @T
	WHERE [ValidTill] >= @PeriodStart AND [ValidFrom] <= @PeriodEnd

	RETURN
END
GO
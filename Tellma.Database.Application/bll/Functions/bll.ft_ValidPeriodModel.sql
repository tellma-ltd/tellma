CREATE FUNCTION [bll].[ft_ValidPeriodModel]
(
	@AccountTypeConcept NVARCHAR (255),
	@AgentDefinitionCode NVARCHAR (255),
	@ResourceDefinitionCode NVARCHAR (255),
	@NotedAgentDefinitionCode NVARCHAR (255),
	@NotedResourceDefinitionCode NVARCHAR (255),
	@LineType TINYINT, -- 20: T for P, 40: Plan, 60:T for T, 80:Template, 100: Event, 120: Regulatory
	@PeriodStart DATE,
	@PeriodEnd DATE
)
RETURNS @MyResult TABLE (
	[Id] INT,
	[LineId] INT,
	[Index] INT,
	[CenterId] INT,
	[AgentId]	INT,
	[AccountId] INT,
	[CurrencyId] NCHAR (3),
	[ResourceId] INT,
	[DurationUnitId] INT,
	[NotedAgentId] INT,
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
	DECLARE @AccountTypeNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(@AccountTypeConcept);
	DECLARE @AgentDefinitionId INT = dal.fn_AgentDefinitionCode__Id(@AgentDefinitionCode);
	DECLARE @ResourceDefinitionId INT = dal.fn_ResourceDefinitionCode__Id(@ResourceDefinitionCode);
	DECLARE @NotedAgentDefinitionId INT = dal.fn_AgentDefinitionCode__Id(@NotedAgentDefinitionCode);
	DECLARE @NotedResourceDefinitionId INT = dal.fn_ResourceDefinitionCode__Id(@NotedResourceDefinitionCode);
	
	DECLARE  @T TABLE (
		[Id] INT PRIMARY KEY,
		[LineId] INT,
		[Index] INT,
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
		[NotedAgentId] INT,
		[NotedResourceId] INT,
		[Quantity] DECIMAL (19, 6),
		[MonetaryValue] DECIMAL (19,6),
		[IsProrated] BIT,
		[EmployeeId] INT,
		[CustomerId] INT,
		[SupplierId] INT
	)
	INSERT INTO @T([Id], [LineId], [Index], [VTime1], [NextTime], [VTime2], [CenterId], [AgentId], [AccountId], [CurrencyId], [ResourceId], [Quantity],
		[DurationUnitId], [NotedAgentId], [NotedResourceId], [MonetaryValue], [IsProrated], [EmployeeId], [CustomerId], [SupplierId])
	SELECT
		E.[Id], E.[LineId], E.[Index],
		E.[Time1] AS VTime1,
		LEAD(E.[Time1], 1, N'9999.12.31') OVER (
			PARTITION BY E.[AccountId], E.[AgentId], E.[ResourceId], E.[NotedAgentId], E.[NotedResourceId], L.[Boolean1], L.[EmployeeId], L.[CustomerId], L.[SupplierId]
			ORDER BY E.[Time1]
		) As [NextTime],
		ISNULL(E.[Time2], N'9999.12.31') AS VTime2,
		E.[CenterId], E.[AgentId], E.[AccountId], E.[CurrencyId], E.[ResourceId], E.[Quantity], E.[DurationUnitId], E.[NotedAgentId], E.[NotedResourceId], E.[MonetaryValue],
		ISNULL(L.[Boolean1], 0) AS IsProrated, L.[EmployeeId], L.[CustomerId], L.[SupplierId]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.AccountTypeId
	LEFT JOIN dbo.Agents AG ON AG.[Id] = E.[AgentId]
	LEFT JOIN dbo.Agents NAG ON NAG.[Id] = E.[NotedAgentId]
	LEFT JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	LEFT JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
	WHERE LD.[LineType] = @LineType
	AND L.[State] = @State
	AND (AC.[Node].IsDescendantOf(@AccountTypeNode) = 1)
	AND (@AgentDefinitionId IS NULL	OR AG.[DefinitionId] = @AgentDefinitionId)
	AND (@NotedAgentDefinitionId IS NULL OR NAG.[DefinitionId] = @NotedAgentDefinitionId)
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
	
	UPDATE @T SET [MonetaryValue] = [Quantity] * [MonetaryValue] WHERE [IsProrated] = 1

	INSERT INTO @MyResult([Id], [LineId], [Index], [CenterId], [AgentId], [AccountId], [CurrencyId], [ResourceId], [DurationUnitId], [NotedAgentId], [NotedResourceId], [ValidFrom],	[ValidTill],[Quantity], [MonetaryValue], [EmployeeId], [CustomerId], [SupplierId])
	SELECT [Id], [LineId], [Index], [CenterId], [AgentId], [AccountId], [CurrencyId], [ResourceId], [DurationUnitId], [NotedAgentId], [NotedResourceId], [ValidFrom], [ValidTill],[Quantity], [MonetaryValue], [EmployeeId], [CustomerId], [SupplierId]
	FROM @T
	WHERE [ValidTill] >= @PeriodStart AND [ValidFrom] <= @PeriodEnd

	RETURN
END
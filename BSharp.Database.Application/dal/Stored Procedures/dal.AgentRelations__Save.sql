CREATE PROCEDURE [dal].[AgentRelations__Save]
	@DefinitionId NVARCHAR (50),
	@Entities dbo.[AgentRelationList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[AgentRelations] AS t
		USING (
			SELECT 	
				[Index],
				[Id],
				[OperatingSegmentId],
				@DefinitionId AS [DefinitionId],
				[AgentId], 
				[StartDate],
				[Code],
				--[CreditLine],
				[JobId],
				[BasicSalary],
				[TransportationAllowance],
				[OvertimeRate],
				[BankAccountNumber],
				[CostObjectType]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[OperatingSegmentId]		=	s.[OperatingSegmentId],
				t.[DefinitionId]			=	s.[DefinitionId],
				t.[AgentId]					=	s.[AgentId],
				t.[StartDate]				=	s.[StartDate],
				t.[Code]					=	s.[Code],
				--t.[CreditLine]				=	s.[CreditLine],
				t.[JobId]					=	s.[JobId],
				t.[BasicSalary]				=	s.[BasicSalary],
				t.[TransportationAllowance] = s.[TransportationAllowance],
				t.[OvertimeRate]			=	s.[OvertimeRate],
				t.[BankAccountNumber]		=	s.[BankAccountNumber],
				t.[CostObjectType]			=	s.[CostObjectType],
				t.[ModifiedAt]				=	@Now,
				t.[ModifiedById]			=	@UserId
		WHEN NOT MATCHED THEN
			INSERT (
				[OperatingSegmentId],
				[DefinitionId],
				[AgentId],
				[StartDate],
				[Code],
				--[CreditLine],
				[JobId],
				[BasicSalary],
				[TransportationAllowance],
				[OvertimeRate],
				[BankAccountNumber],
				[CostObjectType]
			)
			VALUES (
				s.[OperatingSegmentId],
				s.[DefinitionId],
				s.[AgentId],
				s.[StartDate],
				s.[Code],
				--s.[CreditLine],
				s.[JobId],
				s.[BasicSalary],
				s.[TransportationAllowance],
				s.[OvertimeRate],
				s.[BankAccountNumber],
				s.[CostObjectType]
			)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
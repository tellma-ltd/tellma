CREATE PROCEDURE [dal].[AgentRelations__Save]
	@DefinitionId NVARCHAR (255),
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
				[AgentId], 
				[StartDate],
				[Code],
				[CreditLine],
				[BasicSalary],
				[TransportationAllowance],
				[OvertimeRate]
			FROM @Entities 
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[AgentId]			=	s.[AgentId],
				t.[StartDate]		=	s.[StartDate],
				t.[Code]			=	s.[Code],
				t.[CreditLine]		=	s.[CreditLine],
				t.[BasicSalary]		=	s.[BasicSalary],
				t.[TransportationAllowance] = s.[TransportationAllowance],
				t.[OvertimeRate]	=	s.[OvertimeRate],
				t.[ModifiedAt]		=	@Now,
				t.[ModifiedById]	=	@UserId
		WHEN NOT MATCHED THEN
			INSERT ([AgentRelationDefinitionId],
				[AgentId],
				[StartDate],
				[Code],
				[CreditLine],
				[BasicSalary],
				[TransportationAllowance],
				[OvertimeRate]
			)
			VALUES (@DefinitionId,
				s.[AgentId],
				s.[StartDate],
				s.[Code],
				s.[CreditLine],
				s.[BasicSalary],
				s.[TransportationAllowance],
				s.[OvertimeRate]
			)
			OUTPUT s.[Index], inserted.[Id]
	) AS x;

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
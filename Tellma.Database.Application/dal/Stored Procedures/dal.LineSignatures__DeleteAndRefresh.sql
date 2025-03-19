CREATE PROCEDURE [dal].[LineSignatures__DeleteAndRefresh]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @LineIds [dbo].[IdList];

	-- For last signed by same user, soft delete the signature
	UPDATE [dbo].[LineSignatures]
	SET [RevokedAt] = @Now,
		[RevokedById] = @UserId
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
	AND (	
			[RuleType] = 'Public' OR
			[RuleType] IN ('ByUser', 'ByCustodian') AND ([OnBehalfOfUserId] = @UserId OR [CreatedById] = @UserId) OR 
			[RuleType] = 'ByRole' AND [RoleId] IN (SELECT [RoleId] FROM dbo.[RoleMemberships]
													WHERE [UserId] = @UserId
													AND [ValidTo] > GETDATE())
		)

	-- Refresh the lines states
	INSERT INTO @LineIds([Id])
	SELECT DISTINCT [LineId]
	FROM [dbo].[LineSignatures]
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
	
	WITH NewLineStates AS
	(
		SELECT LineId, ToState
		FROM [map].[LinesRequiredSignatures](@LineIds, @UserId)
		WHERE [ToState] < 0
		UNION
		(
			-- consider the positive states only if there are no null signatures or other negative states
			SELECT [LineId], [ToState]
			FROM [map].[LinesRequiredSignatures](@LineIds, @UserId)
			WHERE [ToState] > 0
			EXCEPT 
			SELECT [LineId], ABS([ToState])
			FROM map.[LinesRequiredSignatures](@LineIds, @UserId)
			WHERE [SignedById] is null OR [ToState] < 0
		)
	),
	FinalLineStates AS
	(
		SELECT [LineId], MAX([ToState]) AS [ToState]
		FROM NewLineStates
		GROUP BY [LineId]
	)
	UPDATE L
	SET L.[State] = ISNULL(NL.ToState, 0)
	FROM [Lines] L LEFT JOIN FinalLineStates NL ON L.[Id] = NL.[LineId]
	WHERE L.[Id] IN (SELECT [Id] FROM @LineIds)
	-- refresh the document states
	DECLARE @DocIds [dbo].[IdList];
	INSERT INTO @DocIds([Id])
	SELECT DISTINCT [DocumentId] FROM [dbo].[Lines]
	WHERE [Id] IN (SELECT [Id] FROM @LineIds);

	IF @ReturnIds = 1
		SELECT [Id] FROM @DocIds;
END;
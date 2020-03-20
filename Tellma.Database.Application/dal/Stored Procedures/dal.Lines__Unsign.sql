CREATE PROCEDURE [dal].[Lines__Unsign]
-- TODO: pass signature Ids as input instead of Line ids
	@Ids [dbo].[IdList] READONLY -- currently, these are Ids of Lines 
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- For last signed by same user, soft delete the signature
	UPDATE dbo.[LineSignatures]
	SET [RevokedAt] = @Now,
		[RevokedById] = @UserId
	WHERE [SignedAt] IN (
		SELECT Max([SignedAt]) FROM dbo.[LineSignatures]
		WHERE [LineId] IN (SELECT [Id] FROM @Ids)
		AND RevokedById IS NULL
	)
	AND [LineId] IN (SELECT [Id] FROM @Ids)
	AND [OnBehalfOfUserId] = @UserId;


	-- TODO: Move this logic to dal.Lines__UnsignAndRefresh to look similar to SignAndRefresh
	WITH NewLineStates AS
	(
		SELECT LineId, ToState
		FROM map.[LinesRequiredSignatures](@Ids)
		WHERE ToState < 0
		UNION
		(
			-- consider the positive states only if there are no null signatures or other negative states
			SELECT LineId, ToState
			FROM map.[LinesRequiredSignatures](@Ids)
			WHERE ToState > 0
			EXCEPT 
			SELECT LineId, ABS(ToState)
			FROM map.[LinesRequiredSignatures](@Ids)
			WHERE SignedById is null OR ToState < 0
		)
	),
	FinalLineStates AS
	(
		SELECT LineId, MAX(ToState) AS ToState
		FROM NewLineStates
		GROUP BY LineId
	)
	UPDATE L
	SET L.[State] = ISNULL(NL.ToState, 0)
	FROM Lines L LEFT JOIN FinalLineStates NL ON L.[Id] = NL.[LineId]
	WHERE L.[Id] IN (SELECT [Id] FROM @Ids)
END;
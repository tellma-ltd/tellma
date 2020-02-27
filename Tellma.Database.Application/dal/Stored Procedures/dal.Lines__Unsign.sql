CREATE PROCEDURE [dal].[Lines__Unsign]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- soft delete the signature
	UPDATE dbo.[LineSignatures]
	SET [RevokedAt] = @Now
	WHERE [OnBehalfOfUserId] = @UserId
	AND [Id] IN (SELECT [Id] FROM @Ids);

	-- and last signed by same user, hard delete the signature
	DELETE FROM dbo.[LineSignatures]
	WHERE [SignedAt] IN (
		SELECT Max([SignedAt]) FROM dbo.[LineSignatures]
		WHERE [LineId] IN (SELECT [Id] FROM @Ids)
	)
	AND [OnBehalfOfUserId] = @UserId;

	WITH NewLineStates AS
	(
		SELECT LineId, ToState
		FROM map.RequiredSignatures(@Ids)
		WHERE TOState > 0
		EXCEPT 
		SELECT LineId, ToState
		FROM map.RequiredSignatures(@Ids)
		WHERE SignedById is null
	),
	FinalLineStates AS
	(
		SELECT LineId, MAX(ToState) AS ToState
		FROM NewLineStates
		GROUP BY LineId
	)
	UPDATE L
	SET L.[State] = NL.ToState
	FROM Lines L JOIN NewLineStates NL ON L.[Id] = NL.[LineId]
END;
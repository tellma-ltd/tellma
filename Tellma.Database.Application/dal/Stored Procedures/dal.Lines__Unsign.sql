CREATE PROCEDURE [dal].[Lines__Unsign]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- if last signed by same user, hard delete the signature
	DELETE FROM dbo.[LineSignatures]
	WHERE [Id] IN (
		SELECT Max(Id) FROM dbo.[LineSignatures]
		WHERE [LineId] IN (SELECT [Id] FROM @Ids)
	)
	AND [OnBehalfOfUserId] = @UserId;

	-- else, soft delete the signature
	UPDATE dbo.[LineSignatures]
	SET [RevokedAt] = @Now
	WHERE [OnBehalfOfUserId] = @UserId;
END;
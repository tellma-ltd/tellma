CREATE PROCEDURE [dal].[DocumentLines__Unsign]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- if last signed by same user, hard delete the signature
	DELETE FROM dbo.[DocumentLineSignatures]
	WHERE [Id] IN (
		SELECT Max(Id) FROM dbo.[DocumentLineSignatures]
		WHERE DocumentLineId IN (SELECT [Id] FROM @Ids)
	)
	AND [AgentId] = @UserId;

	-- else, soft delete the signature
	UPDATE dbo.[DocumentLineSignatures]
	SET [RevokedAt] = @Now
	WHERE [AgentId] = @UserId;
END;
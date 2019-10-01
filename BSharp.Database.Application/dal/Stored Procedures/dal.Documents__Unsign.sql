CREATE PROCEDURE [dal].[Documents__Unsign]
	@Documents [dbo].[IdList] READONLY
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- if last signed by same user, hard delete the signature
	DELETE FROM dbo.[DocumentSignatures]
	WHERE [Id] IN (
		SELECT Max(Id) FROM dbo.[DocumentSignatures]
		WHERE DocumentId IN (SELECT [Id] FROM @Documents)
	)
	AND [AgentId] = @UserId;

	-- else, soft delete the signature
	UPDATE dbo.[DocumentSignatures]
	SET [RevokedAt] = @Now
	WHERE [AgentId] = @UserId;
END;
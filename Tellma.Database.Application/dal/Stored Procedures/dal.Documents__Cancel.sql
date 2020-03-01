CREATE PROCEDURE [dal].[Documents__Cancel]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	UPDATE dbo.Documents
	SET
		[State] = -5,
		[PostingStateAt] = SYSDATETIMEOFFSET()
	Where [Id] IN (
		SELECT [Id] FROM @Ids
	);
END;
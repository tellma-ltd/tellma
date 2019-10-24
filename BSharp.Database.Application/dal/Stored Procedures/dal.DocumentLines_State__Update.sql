CREATE PROCEDURE [dal].[DocumentLines_State__Update]
	@Ids [dbo].[IdList] READONLY,
	@ToState NVARCHAR (30)
AS
BEGIN
	UPDATE dbo.DocumentLines
	SET
		[State] = @ToState
	Where [Id] IN (
		SELECT [Id] FROM @Ids
	);
END;
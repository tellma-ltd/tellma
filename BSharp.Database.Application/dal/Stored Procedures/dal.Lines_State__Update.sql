CREATE PROCEDURE [dal].[Lines_State__Update]
	@Ids [dbo].[IdList] READONLY,
	@ToState SMALLINT --NVARCHAR (30)
AS
BEGIN
	UPDATE dbo.[Lines]
	SET
		[State] = @ToState
	Where [Id] IN (
		SELECT [Id] FROM @Ids
	);
END;
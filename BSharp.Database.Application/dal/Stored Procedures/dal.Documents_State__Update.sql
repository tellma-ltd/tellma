CREATE PROCEDURE [dal].[Documents_State__Update]
	@Ids [dbo].[IdList] READONLY,
	@State NVARCHAR (30)
AS
BEGIN
	UPDATE dbo.Documents
	SET
		[State] = @State
	Where [Id] IN (
		SELECT [Id] FROM @Ids
	);
END;
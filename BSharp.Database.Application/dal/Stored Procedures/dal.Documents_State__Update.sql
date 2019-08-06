CREATE PROCEDURE [dal].[Documents_State__Update]
	@Entities [dbo].[IdList] READONLY,
	@State NVARCHAR (30)
AS
BEGIN
	UPDATE dbo.Documents
	SET
		[State] = @State
	Where [Id] IN (
		SELECT [Id] FROM @Entities
	);
END;
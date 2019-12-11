CREATE PROCEDURE [dal].[Documents__Close]
	@Ids [dbo].[IdList] READONLY
AS
BEGIN
	UPDATE dbo.Documents
	SET
		[State] = 5
	Where [Id] IN (
		SELECT [Id] FROM @Ids
	);
END;
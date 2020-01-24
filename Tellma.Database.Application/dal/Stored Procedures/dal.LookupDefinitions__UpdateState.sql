CREATE PROCEDURE [dal].[LookupDefinitions__UpdateState]
	@Ids [dbo].[StringList] READONLY,
	@State NVARCHAR(50)
AS
	UPDATE [dbo].[LookupDefinitions] SET [State] = @State WHERE [Id] IN (SELECT [Id] FROM @Ids);

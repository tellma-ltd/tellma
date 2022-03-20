CREATE PROCEDURE [dal].[EmailTemplates__SetIsError]
	@Id INT
AS
BEGIN
	UPDATE [dbo].[EmailTemplates] SET [IsError] = 1 WHERE [Id] = @Id;
END

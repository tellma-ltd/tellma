CREATE PROCEDURE [dal].[MessageTemplates__SetIsError]
	@Id INT
AS
BEGIN
	UPDATE [dbo].[MessageTemplates] SET [IsError] = 1 WHERE [Id] = @Id;
END

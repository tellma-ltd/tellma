CREATE PROCEDURE [dal].[MessageTemplates__UpdateLastExecuted]
	@Id INT,
	@LastExecuted DATETIMEOFFSET(7)
AS
BEGIN
	UPDATE [dbo].[MessageTemplates] SET [LastExecuted] = @LastExecuted WHERE [Id] = @Id;
END

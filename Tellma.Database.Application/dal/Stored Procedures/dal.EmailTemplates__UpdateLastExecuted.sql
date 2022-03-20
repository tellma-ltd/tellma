CREATE PROCEDURE [dal].[EmailTemplates__UpdateLastExecuted]
	@Id INT,
	@LastExecuted DATETIMEOFFSET(7)
AS
BEGIN
	UPDATE [dbo].[EmailTemplates] SET [LastExecuted] = @LastExecuted WHERE [Id] = @Id;
END

CREATE PROCEDURE [dal].[IdentityServerClients__UpdateSecret]
	@Id INT,
	@ClientSecret NVARCHAR (255),
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	UPDATE [dbo].[IdentityServerClients]
	SET [ClientSecret] = @ClientSecret, [ModifiedById] = @UserId, [ModifiedAt] = @Now
	WHERE [Id] = @Id;
END

CREATE PROCEDURE [dal].[IdentityServerClients__FindByClientId]
	@ClientId NVARCHAR (255),
	@DbClientId NVARCHAR (255) OUTPUT,
	@DbClientSecret NVARCHAR (255) OUTPUT
AS
BEGIN
	SELECT @DbClientId = [ClientId], @DbClientSecret = [ClientSecret] FROM [dbo].[IdentityServerClients]
	WHERE [ClientId] = @ClientId;
END

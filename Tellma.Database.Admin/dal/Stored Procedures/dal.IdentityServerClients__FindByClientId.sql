CREATE PROCEDURE [dal].[IdentityServerClients__FindByClientId]
	@ClientId NVARCHAR (35),
	@DbClientId NVARCHAR (35) OUTPUT,
	@DbClientSecret NVARCHAR (255) OUTPUT
AS
BEGIN
	SELECT @DbClientId = [ClientId], @DbClientSecret = [ClientSecret] FROM [dbo].[IdentityServerClients]
	WHERE [ClientId] = @ClientId;
END

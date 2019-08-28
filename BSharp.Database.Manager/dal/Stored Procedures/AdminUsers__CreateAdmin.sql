CREATE PROCEDURE [dal].[AdminUsers__CreateAdmin]
	@Email NVARCHAR(255),
	@FullName NVARCHAR(255),
	@Password NVARCHAR(255)
AS
IF NOT EXISTS (SELECT * FROM [dbo].[AdminUsers] WHERE [Email] = @Email)
BEGIN
	INSERT INTO [dbo].[AdminUsers] ([Name], [Email], [CreatedById], [ModifiedById])
	VALUES (@FullName, @Email, IDENT_CURRENT('dbo.AdminUsers'), IDENT_CURRENT('dbo.AdminUsers'));

	-- TODO: Assign Administrator privilages to this user
END

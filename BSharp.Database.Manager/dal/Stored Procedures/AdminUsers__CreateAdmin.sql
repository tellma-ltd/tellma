CREATE PROCEDURE [dal].[AdminUsers__CreateAdmin]
	@Email NVARCHAR(255),
	@FullName NVARCHAR(255),
	@Password NVARCHAR(255),
	@AdminServerDescription NVARCHAR(1024) = NULL
AS
IF NOT EXISTS (SELECT * FROM [dbo].[AdminUsers] WHERE [Email] = @Email)
BEGIN
	INSERT INTO [dbo].[AdminUsers] ([Name], [Email], [CreatedById], [ModifiedById])
	VALUES (@FullName, @Email, IDENT_CURRENT('dbo.AdminUsers'), IDENT_CURRENT('dbo.AdminUsers'));

	-- TODO: Assign Administrator privilages to this user
END
IF NOT EXISTS (SELECT * FROM [dbo].[SqlServers] WHERE ServerName = N'<AdminServer>')
BEGIN
	DECLARE @AdminId INT, @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();
	SELECT @AdminId = Id FROM [dbo].[AdminUsers] WHERE [Email] = @Email;

	INSERT INTO SqlServers (ServerName, UserName, PasswordKey, Description, CreatedAt, CreatedById, ModifiedAt, ModifiedById)
	VALUES (N'<AdminServer>', N'', NULL, @AdminServerDescription, @Now, @AdminId, @Now, @AdminId)
END

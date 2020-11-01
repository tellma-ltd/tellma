DELETE FROM @Roles; DELETE FROM @Members

INSERT INTO @Roles([Index],[Id], [Code],[Name],[Name2],[Name3],[IsPublic])
SELECT [Id], [Id], [Code],[Name],[Name2],[Name3],[IsPublic]
FROM dbo.Roles;

INSERT INTO @Members
	([Index],	[HeaderIndex],		[UserId])
SELECT	0,		@AdministratorRL,	Id FROM dbo.Users WHERE [Name] = N'Jiad Akra'
UNION
SELECT	1,		@AdministratorRL,	Id FROM dbo.Users WHERE [Name] = N'Mohamad Akra'
UNION
SELECT	2,		@AdministratorRL,	Id FROM dbo.Users WHERE [Name] = N'Ahmad Akra';

--IF @101MohamadAkra <> @AdminUserId
--	INSERT INTO @Members([Index],[HeaderIndex],	[UserId])
--	VALUES(10,@AdministratorRL,@101MohamadAkra);
--IF @101AhmadAkra <> @AdminUserId
--	INSERT INTO @Members([Index],[HeaderIndex],	[UserId])
--	VALUES(11,@AdministratorRL,@101AhmadAkra);

INSERT INTO @Permissions([Index], [HeaderIndex], [Id], [View], [Action], [Criteria], [Mask], [Memo])
SELECT [Id], [RoleId], [Id], [View], [Action], [Criteria], [Mask], [Memo]
FROM dbo.[Permissions];

EXEC api.Roles__Save
	@Entities = @Roles,
	@Members = @Members,
	@Permissions = @Permissions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Permissions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
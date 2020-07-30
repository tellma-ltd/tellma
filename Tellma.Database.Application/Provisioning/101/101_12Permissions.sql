DELETE FROM @Roles;

INSERT INTO @Roles([Index],[Id], [Code],[Name],[Name2],[Name3],[IsPublic])
SELECT [Id], [Id], [Code],[Name],[Name2],[Name3],[IsPublic]
FROM dbo.Roles;

INSERT INTO @Members
	([Index],	[HeaderIndex],		[UserId]) VALUES
	(0,			@AdministratorRL,	@AdminUserId),
	(5,			@GeneralManagerRL,	@amtaam),
	(0,			@FinanceManagerRL,	@Jiad_akra),
	(0,			@ComptrollerRL,		@alaeldin),
	(0,			@AdminAffairsRL,	@omer)

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
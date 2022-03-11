INSERT INTO @Roles([Index],[Code],[Name],[IsPublic]) VALUES
(0, N'Administrator', N'Administrator', 0),
(98, N'Reader', N'Reader', 0),
(99, N'Public', N'Public', 1);

INSERT INTO @Members([Index], [HeaderIndex], [UserId]) VALUES(0, 0, @AdminUserId);

INSERT INTO @Permissions([Index], [HeaderIndex],
		[Action],	[Criteria],	[View]) VALUES
 (0,0,	N'All',		NULL,		N'all'),
 (0,98,	N'Read',	NULL,		N'all') ;

INSERT INTO @ValidationErrors
EXEC [api].[Roles__Save]
	@Entities = @Roles,
	@Members = @Members,
	@Permissions = @Permissions,
	@ReturnIds = 0,
	@UserId = @AdminUserId;

IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'Roles: Error Provisioning'
	GOTO Err_Label;
END;
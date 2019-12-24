/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/
DECLARE @Accountant INT, @Comptroller INT;
IF NOT EXISTS(SELECT * FROM dbo.Roles WHERE [Name] = N'Accountant')
BEGIN
	INSERT INTO dbo.Roles([Name], [Name2]) VALUES

	(N'Accountant', N'محاسب'),
	(N'Comptroller', N'رئيس حسابات');
	SELECT @Accountant = [Id] FROM dbo.[Roles] WHERE [Name] = N'Accountant';
	SELECT @Comptroller = [Id] FROM dbo.[Roles] WHERE [Name] = N'Comptroller';

	INSERT INTO dbo.RoleMemberships([UserId], [RoleId]) VALUES
	(@UserId, @Accountant),
	(@UserId, @Comptroller);
END;
IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	PRINT N'BSharp.' + @DB;

END
IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	PRINT N'BSharp.' + @DB;

END
IF @DB = N'103' -- Lifan Cars, SAR, en/ar/cn
BEGIN
	PRINT N'BSharp.' + @DB;

END
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	PRINT N'BSharp.' + @DB;
END

IF @DebugRoles = 1
	SELECT * FROM map.Roles();
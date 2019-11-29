/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/
DECLARE @Accountant INT, @Comptroller INT;
INSERT INTO dbo.Roles([Name], [Name2]) VALUES
(N'Accountant', N'محاسب'),
(N'Comptroller', N'رئيس حسابات');
SELECT @Accountant = [Id] FROM dbo.[Roles] WHERE [Name] = N'Accountant';
SELECT @Comptroller = [Id] FROM dbo.[Roles] WHERE [Name] = N'Comptroller';

INSERT INTO dbo.RoleMemberships([UserId], [RoleId]) VALUES
(@UserId, @Accountant),
(@UserId, @Comptroller);

IF @DebugRoles = 1
	SELECT * FROM map.Roles();
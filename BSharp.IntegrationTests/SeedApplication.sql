-- This file is executed before any test is run

DECLARE @UserId INT, @RoleId INT;
SELECT @UserId = [Id] FROM [dbo].[Users] WHERE [Email] = @Email;
SELECT @RoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'Administrator';

EXEC sp_set_session_context 'UserId', @UserId;

DELETE FROM [dbo].[Permissions];
DELETE FROM [dbo].[RoleMemberships];

DELETE FROM [dbo].[Roles] WHERE [Id] <> @RoleId;
DELETE FROM [dbo].[Users] WHERE [Id] <> @UserId;
DELETE FROM [dbo].[Agents] WHERE [Id]<> @UserId;
DELETE FROM [dbo].[Currencies] WHERE [Id] NOT IN (N'ETB', N'USD');


DELETE FROM [dbo].[MeasurementUnits];
DELETE FROM [dbo].[ProductCategories];
DELETE FROM [dbo].[ResourceLookups];

INSERT INTO [dbo].[Permissions] ([RoleId], [ViewId], [Action])
VALUES (@RoleId, N'agents', N'All'),
(@RoleId, N'users', N'All'),
(@RoleId, N'roles', N'All'),
(@RoleId, N'views', N'All')


INSERT INTO [dbo].[RoleMemberships] ([AgentId], [RoleId])
VALUES (@UserId, @RoleId)

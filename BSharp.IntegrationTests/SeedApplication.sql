-- This file is executed before any test is run

DECLARE @UserId INT, @RoleId INT;
SELECT @UserId = [Id] FROM [dbo].[Users] WHERE [Email] = @Email;
SELECT @RoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'Administrator';

EXEC sp_set_session_context 'UserId', @UserId;

-- Cleanup, Central records before lookup records

DELETE FROM [dbo].[Permissions];
DELETE FROM [dbo].[RoleMemberships];

DELETE FROM [dbo].[Accounts];

DELETE FROM [dbo].[Roles] WHERE [Id] <> @RoleId;
DELETE FROM [dbo].[Users] WHERE [Id] <> @UserId;
DELETE FROM [dbo].[Agents];
DELETE FROM [dbo].[Resources];
DELETE FROM [dbo].[Currencies];
--DELETE FROM [dbo].[ResourceClassifications] WHERE [Code] NOT IN (N'CashAndCashEquivalents');

DELETE FROM [dbo].[ResourceClassifications];
DELETE FROM [dbo].[Lookups];
DELETE FROM [dbo].[MeasurementUnits];
DELETE FROM [dbo].[AccountClassifications];
DELETE FROM [dbo].[ResourceDefinitions];
DELETE FROM [dbo].[ResponsibilityCenters];

-- Populate

INSERT INTO [dbo].[Permissions] ([RoleId], [ViewId], [Action])
VALUES
(@RoleId, N'users', N'All'),
(@RoleId, N'roles', N'All')


INSERT INTO [dbo].[RoleMemberships] ([UserId], [RoleId])
VALUES (@UserId, @RoleId)


IF NOT EXISTS(SELECT * FROM dbo.ResourceDefinitions WHERE [Id] = N'Basic')
INSERT INTO dbo.ResourceDefinitions (
	[Id],	[TitlePlural],	[TitleSingular]) VALUES
(N'Basic',	N'Items',		N'Item');

-- Resource Types
DECLARE @ResourceClassifications dbo.ResourceClassificationList
INSERT INTO @ResourceClassifications
([Code],									[Name],											[Node],		[IsAssignable], [Index]) VALUES
(N'CashAndCashEquivalents',					N'Cash and cash equivalents',					N'/1/13/',		1,				0),
	(N'Cash',								N'Cash',										N'/2/1/1/',		1,1),
	(N'CashEquivalents',					N'Cash equivalents',							N'/2/1/2/',		1,2);

DECLARE @ValidationErrorsJson nvarchar(max);
EXEC [api].[ResourceClassifications__Save]
	@Entities = @ResourceClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;


-- Currencies
DECLARE @Currencies CurrencyList;
INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [E]) VALUES
(0, N'USD', N'US Dollar',N'دولار أمريكي', 2),
(1, N'ETB', N'ET Birr', N'بر أثيوبي', 2);

EXEC dal.Currencies__Save
	@Entities = @Currencies;

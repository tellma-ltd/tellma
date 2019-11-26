-- This file is executed before any test is run

DECLARE @UserId INT, @RoleId INT;
SELECT @UserId = [Id] FROM [dbo].[Users] WHERE [Email] = @Email;
SELECT @RoleId = [Id] FROM [dbo].[Roles] WHERE [Name] = 'Administrator';

EXEC sp_set_session_context 'UserId', @UserId;

-- Cleanup, Central accounts before lookup accounts

DELETE FROM [dbo].[Permissions];
DELETE FROM [dbo].[RoleMemberships];

DELETE FROM [dbo].[Accounts];

DELETE FROM [dbo].[Roles] WHERE [Id] <> @RoleId;
DELETE FROM [dbo].[Users] WHERE [Id] <> @UserId;
DELETE FROM [dbo].[Agents] WHERE [Id]<> @UserId;
DELETE FROM [dbo].[Resources];
DELETE FROM [dbo].[Currencies];
DELETE FROM [dbo].[ResourceTypes] WHERE [Id] NOT IN (N'CashAndCashEquivalents');

DELETE FROM [dbo].[ResourceClassifications];
DELETE FROM [dbo].[Lookups];
DELETE FROM [dbo].[MeasurementUnits];
DELETE FROM [dbo].[ResourceClassifications];
DELETE FROM [dbo].[AccountClassifications];
DELETE FROM [dbo].[ResourceDefinitions];
DELETE FROM [dbo].[ResponsibilityCenters];

-- Populate

INSERT INTO [dbo].[Permissions] ([RoleId], [ViewId], [Action])
VALUES (@RoleId, N'agents', N'All'),
(@RoleId, N'users', N'All'),
(@RoleId, N'roles', N'All'),
(@RoleId, N'views', N'All')


INSERT INTO [dbo].[RoleMemberships] ([AgentId], [RoleId])
VALUES (@UserId, @RoleId)

INSERT INTO [dbo].[ResourceDefinitions] ([Id], [TitlePlural], [TitleSingular])
VALUES (N'monetary-resources', N'Monetary Resources', N'Monetary Resource'), 
(N'raw-materials', N'Raw Materials', N'Raw Material');

DECLARE @ResourceTypes AS TABLE (
	[Id]					NVARCHAR (255)		PRIMARY KEY NONCLUSTERED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[Node]					HIERARCHYID			NOT NULL
);
INSERT INTO @ResourceTypes
([Id],										[Name],											[Node],			[IsAssignable], [IsActive]) VALUES
(N'CashAndCashEquivalents',					N'Cash and cash equivalents',					N'/1/13/',		1,				1);

MERGE [dbo].[ResourceTypes] AS t
USING (
		SELECT [Id], [IsAssignable], [Name], [Name2], [Name3], [IsActive], [Node]
		FROM @ResourceTypes
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED
THEN
	UPDATE SET
		t.[IsAssignable]	=	s.[IsAssignable],
		t.[Name]			=	s.[Name],
		t.[Name2]			=	s.[Name2],
		t.[Name3]			=	s.[Name3],
		t.[Node]			=	s.[Node]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE -- to delete Ifrs Resource Classifications extension concepts we added erroneously
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id],	[IsAssignable],		[Name],	[Name2],	[Name3],	[IsActive],	[Node])
    VALUES (s.[Id], s.[IsAssignable], s.[Name], s.[Name2], s.[Name3], s.[IsActive], s.[Node]);

DECLARE @Currencies CurrencyList;
INSERT INTO @Currencies([Index], [Id], [Name], [Name2], [E]) VALUES
(0, N'USD', N'US Dollar',N'دولار أمريكي', 2),
(1, N'ETB', N'ET Birr', N'بر أثيوبي', 2);

EXEC dal.Currencies__Save
	@Entities = @Currencies;

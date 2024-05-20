CREATE FUNCTION [rpt].[ft_TellmaUsage] (
)
RETURNS @returntable TABLE
(
	[ComponentType] NVARCHAR (10),
	[Id]	INT,
	[Component] NVARCHAR (255),
	[Source] NVARCHAR (255),
	[Price]	DECIMAL (19, 6)
	PRIMARY KEY ([ComponentType], [Id], [Component])
)
AS 
BEGIN
DECLARE @UsersEmails StringList;
INSERT INTO @UsersEmails ([Id])
SELECT Email
FROM dbo.Users
WHERE IsService = 0 AND IsActive = 1
AND [Name] NOT LIKE N'%test%'
AND Email NOT LIKE N'%banan-it.com'
AND Email NOT LIKE N'%tellma.com'
AND Email NOT IN (N'mosab.alhaafith@gmail.com', N'amirahakawaty26@gmail.com', N'mkanafani40@gmail.com');

INSERT INTO @returntable([ComponentType], [Id], [Component], [Source])
SELECT [Component Type], [Id], [Component], IIF(MIN([Source]) = MAX([Source]), MIN([Source]), MIN([Source]) + ',' +  MAX([Source])) AS [Source]
FROM (
	SELECT [Component Type], [Id], [Component], [Source] FROM
	(
		SELECT DISTINCT
		CASE 
			WHEN [View] LIKE N'agents/%'	THEN N'Agent'
			WHEN [View] LIKE N'resources/%' THEN N'Resource'
			WHEN [View] LIKE N'documents/%' THEN N'Document'
		END AS [Component Type],
		CASE 
			WHEN [View] LIKE N'agents/%'  THEN (SELECT [Id] FROM dbo.AgentDefinitions WHERE [Id] = (CAST(RIGHT([View], LEN([View]) - LEN(N'agents/')) AS INT)))
			WHEN [View] LIKE N'resources/%'  THEN (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Id] = (CAST(RIGHT([View], LEN([View]) - LEN(N'resources/')) AS INT)))
			WHEN [View] LIKE N'documents/%'  THEN (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Id] = (CAST(RIGHT([View], LEN([View]) - LEN(N'documents/')) AS INT)))
		END AS [Id],
		CASE 
			WHEN [View] LIKE N'agents/%'  THEN (SELECT [TitlePlural] FROM dbo.AgentDefinitions WHERE [Id] = (CAST(RIGHT([View], LEN([View]) - LEN(N'agents/')) AS INT)))
			WHEN [View] LIKE N'resources/%'  THEN (SELECT [TitlePlural] FROM dbo.ResourceDefinitions WHERE [Id] = (CAST(RIGHT([View], LEN([View]) - LEN(N'resources/')) AS INT)))
			WHEN [View] LIKE N'documents/%'  THEN (SELECT [TitlePlural] FROM dbo.DocumentDefinitions WHERE [Id] = (CAST(RIGHT([View], LEN([View]) - LEN(N'documents/')) AS INT)))
		END AS [Component],
		N'Role Access' AS [Source]
		FROM dbo.[Permissions] P
		JOIN (
			SELECT DISTINCT [RoleId]
			FROM dbo.RoleMemberships RM
			JOIN dbo.Users U ON U.[Id] = RM.[UserId]
			JOIN dbo.Roles R ON R.[Id] = RM.[RoleId]
			WHERE U.[Email] IN (SELECT [Id] FROM @UsersEmails)
			AND R.[IsActive] = 1
			UNION ALL
			SELECT DISTINCT [Id]
			FROM dbo.Roles
			WHERE [IsActive] = 1
			AND [IsPublic] = 1
		) R ON R.[RoleId] = P.[RoleId]
	) T 
	WHERE [Component] NOT IN (
		SELECT [TitlePlural] FROM AgentDefinitions
		WHERE [CurrencyVisibility] = N'None'
		AND [CenterVisibility]	= N'None'
		AND [ImageVisibility]		= N'None'
		AND [DescriptionVisibility]			= N'None'
		AND [LocationVisibility]	= N'None'
		AND [FromDateVisibility] = N'None'
		AND	[ToDateVisibility]= N'None'
		AND [DateOfBirthVisibility]		= N'None'	
		AND [ContactEmailVisibility]	= N'None'
		AND [ContactMobileVisibility]	= N'None'
		AND [ContactAddressVisibility]	= N'None'
		AND [Date1Visibility]= N'None' AND [Date2Visibility] = N'None' AND [Date3Visibility]= N'None' AND [Date4Visibility]	= N'None'
		AND [Decimal1Visibility] = N'None' AND [Decimal2Visibility] = N'None'
		AND [Int1Visibility] = N'None' AND [Int2Visibility] = N'None'
		AND [Lookup1Visibility] = N'None' AND [Lookup2Visibility] = N'None' AND [Lookup3Visibility] = N'None' AND [Lookup4Visibility] = N'None'
		AND [Lookup5Visibility] = N'None' AND [Lookup6Visibility] = N'None' AND [Lookup7Visibility] = N'None' AND [Lookup8Visibility] = N'None'
		AND [Text1Visibility] = N'None' AND [Text2Visibility] = N'None' AND [Text3Visibility] = N'None' AND [Text4Visibility] = N'None'
		AND [Agent1Visibility] = N'None' AND [Agent2Visibility] = N'None'
		AND [TaxIdentificationNumberVisibility] = N'None'
		AND [BankAccountNumberVisibility] = N'None'
		AND [ExternalReferenceVisibility] = N'None'
		AND [UserCardinality]= N'None'
		AND [HasAttachments] = 0
		OR [State] <> N'Visible'
	)
	AND [Component] NOT IN (
		SELECT [TitlePlural] FROM ResourceDefinitions
		WHERE [CurrencyVisibility] = N'None'
		AND [CenterVisibility]	= N'None'
		AND [ImageVisibility]		= N'None'
		AND [DescriptionVisibility]			= N'None'
		AND [LocationVisibility]	= N'None'
		AND [FromDateVisibility] = N'None'
		AND	[ToDateVisibility]= N'None'
		AND [Date1Visibility]= N'None' AND [Date2Visibility] = N'None' AND [Date3Visibility]= N'None' AND [Date4Visibility]	= N'None'
		AND [Decimal1Visibility] = N'None' AND [Decimal2Visibility] = N'None' AND [Decimal3Visibility] = N'None' AND [Decimal4Visibility] = N'None'
		AND [Int1Visibility] = N'None' AND [Int2Visibility] = N'None'
		AND [Lookup1Visibility] = N'None' AND [Lookup2Visibility] = N'None' AND [Lookup3Visibility] = N'None' AND [Lookup4Visibility] = N'None'
		AND [Text1Visibility] = N'None' AND [Text2Visibility] = N'None' 
		AND [IdentifierVisibility] = N'None'
		AND [VatRateVisibility] = N'None'
		AND [ReorderLevelVisibility] = N'None'
		AND [EconomicOrderQuantityVisibility] = N'None'
		AND [UnitCardinality] = N'None'
		AND [UnitMassVisibility] = N'None'
		AND [MonetaryValueVisibility] = N'None'
		AND [Agent1Visibility] = N'None' AND [Agent2Visibility] = N'None'
		AND [Resource1Visibility] = N'None' AND [Resource2Visibility] = N'None'
		AND [HasAttachments] = 0
		OR [State] <> N'Visible'
	)

	UNION ALL
	SELECT 'Template' AS [Component Type], [Id], [Name] AS [Component],  N'Deployed/Role Access' AS [Source]
	FROM dbo.PrintingTemplates
	WHERE [Usage] IN (N'FromDetails', N'FromSearchAndDetails') AND [IsDeployed] = 1
	OR [Usage] = N'Standalone' AND [ShowInMainMenu] = 1 AND [Id] IN (
			SELECT DISTINCT [PrintingTemplateId]
			FROM dbo.[PrintingTemplateRoles] PTR
			JOIN dbo.RoleMemberships RM ON RM.[RoleId] = PTR.[RoleId]
			JOIN dbo.Users U ON U.[Id] = RM.[UserId]
			JOIN dbo.Roles R ON R.[Id] = RM.[RoleId]
			WHERE U.[Email] IN (SELECT [Id] FROM @UsersEmails)
			AND R.[IsActive] = 1
			UNION ALL
			SELECT DISTINCT [PrintingTemplateId]
			FROM dbo.[PrintingTemplateRoles] PTR
			JOIN dbo.Roles R ON R.[Id] = PTR.[RoleId]
			WHERE R.[IsActive] = 1
			AND R.[IsPublic] = 1
		)

	UNION ALL
	SELECT 'Template' AS [ComponentType], [Id], [Name] AS [Component], N'Deployed' AS [Source]
	FROM dbo.EmailTemplates
	WHERE [IsDeployed] = 1

	UNION ALL
	SELECT 'Report' AS [Component Type], [Id], [Title] AS [Component], N'Role Access/In Main Menu' AS [Source]
	FROM dbo.ReportDefinitions
	WHERE [ShowInMainMenu] = 1 AND [Id] IN (
			SELECT DISTINCT [ReportDefinitionId]
			FROM dbo.[ReportDefinitionRoles] RDR
			JOIN dbo.RoleMemberships RM ON RM.[RoleId] = RDR.[RoleId]
			JOIN dbo.Users U ON U.[Id] = RM.[UserId]
			JOIN dbo.Roles R ON R.[Id] = RM.[RoleId]
			WHERE U.[Email] IN (SELECT [Id] FROM @UsersEmails)
			AND R.[IsActive] = 1
			UNION ALL
			SELECT DISTINCT [ReportDefinitionId]
			FROM dbo.[ReportDefinitionRoles] RDR
			JOIN dbo.Roles R ON R.[Id] = RDR.[RoleId]
			WHERE R.[IsActive] = 1
			AND R.[IsPublic] = 1
		)

	UNION
	SELECT 'Report' AS [Component Type], [Id], [Title] AS [Component], 'In Dashboard' AS [Source]
	FROM dbo.ReportDefinitions
	WHERE [Id] IN (
		SELECT DISTINCT [ReportDefinitionId]
		FROM [dbo].[DashboardDefinitionWidgets] DDW
		JOIN dbo.[DashboardDefinitions] DD ON DD.[Id] = DDW.[DashboardDefinitionId]
		WHERE DD.[ShowInMainMenu] = 1
	)

	UNION
	SELECT 'Report' AS [Component Type], [Id], [Title] AS [Component], 'Agent/Sub Report' AS [Source]
	FROM dbo.ReportDefinitions
	WHERE [Id] IN (
		SELECT DISTINCT [ReportDefinitionId]
		FROM [dbo].[AgentDefinitionReportDefinitions] ADRD
		JOIN dbo.[AgentDefinitions] AD ON AD.[Id] = ADRD.[AgentDefinitionId]
		WHERE AD.[State] = N'Visible'
		AND AD.[TitlePlural] IN (
			SELECT DISTINCT (SELECT [TitlePlural] FROM dbo.AgentDefinitions WHERE [Id] = (CAST(RIGHT([View], LEN([View]) - LEN(N'agents/')) AS INT)))
			FROM dbo.[Permissions] P
			JOIN (
				SELECT DISTINCT [RoleId]
				FROM dbo.RoleMemberships RM
				JOIN dbo.Users U ON U.[Id] = RM.[UserId]
				JOIN dbo.Roles R ON R.[Id] = RM.[RoleId]
				WHERE U.[Email] IN (SELECT [Id] FROM @UsersEmails)
				AND R.[IsActive] = 1
				UNION ALL
				SELECT DISTINCT [Id]
				FROM dbo.Roles
				WHERE [IsActive] = 1
				AND [IsPublic] = 1
			) R ON R.[RoleId] = P.[RoleId]
			WHERE [View] LIKE  N'agents/%' 
		)
	)

	UNION
	SELECT 'Report' AS [Component Type], [Id], [Title] AS [Component], 'Resource/Sub Report' AS [Source]
	FROM dbo.ReportDefinitions
	WHERE [Id] IN (
		SELECT DISTINCT [ReportDefinitionId]
		FROM [dbo].[ResourceDefinitionReportDefinitions] RDRD
		JOIN dbo.[ResourceDefinitions] RD ON RD.[Id] = RDRD.[ResourceDefinitionId]
		WHERE RD.[State] = N'Visible'
		AND RD.[TitlePlural] IN (
			SELECT DISTINCT (SELECT [TitlePlural] FROM dbo.ResourceDefinitions WHERE [Id] = (CAST(RIGHT([View], LEN([View]) - LEN(N'resources/')) AS INT)))
			FROM dbo.[Permissions] P
			JOIN (
				SELECT DISTINCT [RoleId]
				FROM dbo.RoleMemberships RM
				JOIN dbo.Users U ON U.[Id] = RM.[UserId]
				JOIN dbo.Roles R ON R.[Id] = RM.[RoleId]
				WHERE U.[Email] IN (SELECT [Id] FROM @UsersEmails)
				AND R.[IsActive] = 1
				UNION ALL
				SELECT DISTINCT [Id]
				FROM dbo.Roles
				WHERE [IsActive] = 1
				AND [IsPublic] = 1
			) R ON R.[RoleId] = P.[RoleId]
			WHERE [View] LIKE  N'resources/%' 
		)
	)
) T
GROUP BY [Id], [Component Type], [Component]
ORDER BY [Component Type], [Component];

INSERT INTO @returntable([ComponentType], [Id], [Component], [Source])
SELECT 'Tab' AS [ComponentType],  [Id], TitlePlural, N'Role Access'
FROM dbo.LineDefinitions
WHERE [Id] IN (
	SELECT [LineDefinitionId]
	FROM dbo.DocumentDefinitionLineDefinitions
	WHERE [DocumentDefinitionId] IN (
		SELECT [Id] FROM @returntable
		WHERE [ComponentType] = N'Document'
	)
)

INSERT INTO @returntable([ComponentType], [Id], [Component], [Source])
SELECT N'User' AS [ComponentType],  [Id], [Name], N'Public'
FROM dbo.Users
WHERE [Email] IN (SELECT [Id] FROM @UsersEmails)
AND [Id] IN (
	SELECT [UserId]
	FROM dbo.RoleMemberships RM
	JOIN dbo.Roles R ON R.[Id] = RM.[RoleId]
	AND R.[IsActive] = 1
)

INSERT INTO @returntable([ComponentType], [Id], [Component], [Source])
SELECT N'Self Srv' AS [ComponentType],  [Id], [Name], N'Public'
FROM dbo.Users
WHERE [Email] IN (SELECT [Id] FROM @UsersEmails)
AND [Id] NOT IN (
	SELECT [Id] FROM @returntable
	WHERE [ComponentType] = N'User'
)

UPDATE R -- @returntable
SET [Price] = TC.[Decimal1]
FROM @returntable R
JOIN (
	SELECT CASE 
		WHEN [Name] IN (N'Document', N'Report', N'Tab', N'Template') THEN [Name]
		WHEN [Name] = N'User - Regular' THEN N'User'
		WHEN [Name] = N'User - Self Service' THEN N'Self Srv'
		WHEN [Name] = N'Master Screen' THEN N'Master'
	END [ComponentType], [Decimal1]
	FROM dbo.Resources
	WHERE [DefinitionId] = dal.fn_ResourceDefinitionCode__Id(N'TellmaComponents.Free')
	AND [Name] IN (N'Document', N'Report', N'Tab', N'Template',  N'User - Regular',  N'User - Self Service', N'Master Screen')
) TC ON TC.[ComponentType] = R.[ComponentType] OR (TC.[ComponentType] = N'Master' AND R.[ComponentType] IN (N'Resource', N'Agent'));

-- Free reports
WITH FreeReportDefinitions AS (
	SELECT [Id]
	FROM 
	dbo.ReportDefinitions
	WHERE [Code] LIKE '%.Free'
)
UPDATE R
SET [ComponentType] =  N'FreeReport',
	[Price] = 0
FROM @returntable R
JOIN FreeReportDefinitions RD ON RD.[Id] = R.[Id]
WHERE R.[ComponentType] = N'Report';

-- Free tabs
WITH FreeLineDefinitions AS (
	SELECT [Id]
	FROM 
	dbo.LineDefinitions
	WHERE [Code] LIKE '%.Free' OR [Code] IN (N'ManualLine')
)
UPDATE R
SET [ComponentType] =  N'Free Tab',
	[Price] = 0
FROM @returntable R
JOIN FreeLineDefinitions LD ON LD.[Id] = R.[Id]
WHERE R.[ComponentType] = N'Tab';

-- Free templates
WITH FreeTemplates AS (
	SELECT [Id], [Name]
	FROM 
	dbo.EmailTemplates
	WHERE [Code] LIKE '%.Free'
	UNION ALL
	SELECT [Id], [Name]
	FROM 
	dbo.PrintingTemplates
	WHERE [Code] LIKE '%.Free'
)
UPDATE R
SET [ComponentType] =  N'Free Temp',
	[Price] = 0
FROM @returntable R
JOIN FreeTemplates TD ON TD.[Id] = R.[Id] AND TD.[Name] = R.[Component]
WHERE R.[ComponentType] = N'Template';

-- Free agents
WITH FreeAgentDefinitions AS (
	SELECT [Id]
	FROM 
	dbo.AgentDefinitions
	WHERE [Code] LIKE '%.Free'
)
UPDATE R
SET [ComponentType] = N'Free Agent',
	[Price] = 0
FROM @returntable R
JOIN FreeAgentDefinitions AD ON AD.[Id] = R.[Id]
WHERE R.[ComponentType] = N'Agent';

-- Free resources
WITH FreeResourceDefinitions AS (
	SELECT [Id]
	FROM 
	dbo.ResourceDefinitions
	WHERE [Code] LIKE '%.Free'
)
UPDATE R
SET [ComponentType] =  N'Free Rsrce',
	[Price] = 0
FROM @returntable R
JOIN FreeResourceDefinitions RD ON RD.[Id] = R.[Id]
WHERE R.[ComponentType] = N'Resource';

RETURN
END
GO
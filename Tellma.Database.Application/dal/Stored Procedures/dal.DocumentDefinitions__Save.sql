CREATE PROCEDURE [dal].[DocumentDefinitions__Save]
	@Entities dbo.[DocumentDefinitionList] READONLY,
	@DocumentDefinitionLineDefinitions [DocumentDefinitionLineDefinitionList] READONLY
AS
DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
MERGE [dbo].[DocumentDefinitions] AS t
USING @Entities AS s
ON s.Id = t.Id
WHEN MATCHED THEN
	UPDATE SET
		t.[IsOriginalDocument]	= s.[IsOriginalDocument], 
		t.[TitleSingular]		= s.[TitleSingular],
		t.[TitleSingular2]		= s.[TitleSingular2],
		t.[TitleSingular3]		= s.[TitleSingular3],
		t.[TitlePlural]			= s.[TitlePlural],
		t.[TitlePlural2]		= s.[TitlePlural2],
		t.[TitlePlural3]		= s.[TitlePlural3],
		t.[Prefix]				= s.[Prefix],
		t.[CodeWidth]			= s.[CodeWidth],
		t.[AgentDefinitionId]	= s.[AgentDefinitionId],
		t.[ClearanceVisibility]	= s.[ClearanceVisibility],
		t.[InvestmentCenterVisibility]= s.[InvestmentCenterVisibility],
		t.[Time1Visibility]		= s.[Time1Visibility],
		t.[Time1Label1]			= s.[Time1Label1],	
		t.[Time1Label2]			= s.[Time1Label2],	
		t.[Time1Label3]			= s.[Time1Label3],	
		t.[Time2Visibility]		= s.[Time2Visibility],
		t.[Time2Label1]			= s.[Time2Label1],	
		t.[Time2Label2]			= s.[Time2Label2],	
		t.[Time2Label3]			= s.[Time2Label3],	
		t.[QuantityVisibility]	= s.[QuantityVisibility],
		t.[QuantityLabel1]		= s.[QuantityLabel1],
		t.[QuantityLabel2]		= s.[QuantityLabel2],
		t.[QuantityLabel3]		= s.[QuantityLabel3],
		t.[UnitVisibility]		= s.[UnitVisibility],
		t.[UnitLabel1]			= s.[UnitLabel1],
		t.[UnitLabel2]			= s.[UnitLabel2],
		t.[UnitLabel3]			= s.[UnitLabel3],
		t.[MainMenuIcon]		= s.[MainMenuIcon],
		t.[MainMenuSection]		= s.[MainMenuSection],
		t.[MainMenuSortKey]		= s.[MainMenuSortKey]

WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
		[Id], [IsOriginalDocument], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3],
		[Prefix], [CodeWidth],
		[AgentDefinitionId], 
		[ClearanceVisibility],
		[InvestmentCenterVisibility],
		[Time1Visibility],
		[Time1Label1],	
		[Time1Label2],	
		[Time1Label3],	
		[Time2Visibility],
		[Time2Label1],	
		[Time2Label2],	
		[Time2Label3],	
		[QuantityVisibility],
		[QuantityLabel1],
		[QuantityLabel2],
		[QuantityLabel3],
		[UnitVisibility],
		[UnitLabel1],
		[UnitLabel2],
		[UnitLabel3],
		[MainMenuIcon],		[MainMenuSection], [MainMenuSortKey]
	) VALUES (
		s.[Id], s.[IsOriginalDocument], s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3],
		s.[Prefix], s.[CodeWidth],
		s.[AgentDefinitionId],
		s.[ClearanceVisibility],
		s.[InvestmentCenterVisibility],
		s.[Time1Visibility],
		s.[Time1Label1],	
		s.[Time1Label2],	
		s.[Time1Label3],	
		s.[Time2Visibility],
		s.[Time2Label1],	
		s.[Time2Label2],	
		s.[Time2Label3],	
		s.[QuantityVisibility],
		s.[QuantityLabel1],
		s.[QuantityLabel2],
		s.[QuantityLabel3],
		s.[UnitVisibility],
		s.[UnitLabel1],
		s.[UnitLabel2],
		s.[UnitLabel3],
		s.[MainMenuIcon], s.[MainMenuSection], s.[MainMenuSortKey]
	);

MERGE [dbo].[DocumentDefinitionLineDefinitions] AS t
USING (
	SELECT
		DDLD.[Index],
		DDLD.[Id],
		DD.[Id] AS [DocumentDefinitionId],
		DDLD.[LineDefinitionId],
		DDLD.[IsVisibleByDefault]
	FROM @Entities DD
	JOIN @DocumentDefinitionLineDefinitions DDLD ON DD.[Index] = DDLD.[HeaderIndex]
) AS s
ON s.Id = t.Id
WHEN MATCHED THEN
	UPDATE SET
		t.[Index]				= s.[Index],
		t.[LineDefinitionId]	= s.[LineDefinitionId],
		t.[IsVisibleByDefault]	= s.[IsVisibleByDefault],
		t.[SavedById]			= @UserId
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
		[Index], [DocumentDefinitionId],		[LineDefinitionId], [IsVisibleByDefault]
	) VALUES (
		[Index], s.[DocumentDefinitionId], s.[LineDefinitionId], s.[IsVisibleByDefault]
	);
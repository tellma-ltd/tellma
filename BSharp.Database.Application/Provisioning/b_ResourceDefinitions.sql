IF NOT EXISTS(SELECT * FROM dbo.ResourceDefinitions)
BEGIN
	INSERT INTO dbo.ResourceDefinitions
	([Id],				[TitlePlural],		[TitleSingular]) VALUES
	(N'currencies',		N'Currencies',		N'Currency'),
	(N'general-items',	N'General Items',	N'General Item');
END
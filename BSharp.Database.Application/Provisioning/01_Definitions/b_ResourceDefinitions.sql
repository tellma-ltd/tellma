IF NOT EXISTS(SELECT * FROM dbo.ResourceDefinitions)
BEGIN
	INSERT INTO dbo.ResourceDefinitions
	([Id],				[TitlePlural],		[TitleSingular]) VALUES
	(N'general-items',	N'General Items',	N'General Item');
END
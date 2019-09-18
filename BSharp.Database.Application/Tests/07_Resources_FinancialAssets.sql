	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'financial-assets
							[Name],				[IsLeaf],	[Node]) VALUES
	(N'financial-assets',	N'Checks (received)',1,			N'/1/'),
	(N'financial-assets',	N'CPO (received)',	0,			N'/2/'),
	(N'financial-assets',	N'L/C (received)',	1,			N'/3/'),
	(N'financial-assets',	N'L/G (received)',	1,			N'/4/');
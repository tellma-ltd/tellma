	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'computer-equipment'
							[Name],				[IsLeaf],	[Node]) VALUES
	(N'computer-equipment',	N'Servers',			1,			N'/1/'),
	(N'computer-equipment',	N'Desktops',		1,			N'/2/'),
	(N'computer-equipment',	N'Laptops',			1,			N'/3/'),
	(N'computer-equipment',	N'Mobiles',			1,			N'/4/'),
	(N'computer-equipment',	N'Printers',		0,			N'/5/'),
	(N'computer-equipment',	N'Color printers',	1,			N'/5/1/'),
	(N'computer-equipment',	N'B/W printers',	1,			N'/5/2/');
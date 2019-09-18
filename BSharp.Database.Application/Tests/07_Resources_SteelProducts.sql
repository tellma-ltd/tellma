	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'steel-products'
						[Name],	[IsLeaf],	[Node]) VALUES
	(N'steel-products',	N'D',	1,			N'/1/'),
	(N'steel-products',	N'HSP',	0,			N'/2/'),
	(N'steel-products',	N'CHS',	1,			N'/2/1/'),
	(N'steel-products',	N'RHS',	1,			N'/2/2/'),
	(N'steel-products',	N'SHS',	1,			N'/2/3/'),
	(N'steel-products',	N'LTZ',	0,			N'/3/'),
	(N'steel-products',	N'L',	1,			N'/3/1/'),
	(N'steel-products',	N'T',	1,			N'/3/2/'),
	(N'steel-products',	N'Z',	1,			N'/3/3/'),
	(N'steel-products',	N'SM',	1,			N'/4/'),
	(N'steel-products',	N'CP',	1,			N'/5/'),
	(N'steel-products',	N'X',	1,			N'/6/');
	INSERT INTO dbo.[AccountTypes] ( -- N'financial-assets
							[Name],													[IsLeaf],	[Node]) VALUES
	(N'financial assets at fair value through profit or loss',1,		N'/1/'),
	(N'held-to-maturity investments',						1,			N'/2/'),
	(N'loans and receivables',								1,			N'/3/'),
	(N'Checks (received)',									1,			N'/3/1/'),
	(N'CPO (received)',										1,			N'/3/2/'),
	(N'L/C (received)',										1,			N'/3/3/'),
	(N'L/G (received)',										1,			N'/3/4/'),
	(N'available-for-sale financial assets',				1,			N'/4/')
	;
	--cash, deposits in other entities, trade receivables, loans to other entities. investments in debt instruments,
	-- investments in shares and other equity instruments
INSERT INTO @Users
([Index],	[Name],					[Email]) VALUES
(0,			N'Jiad Akra',			N'jiad.akra@banan-it.com'),
(1,			N'Mohamad Akra',		N'mohamad.akra@tellma.com'),
(2,			N'Ahmad Akra',			N'ahmad.akra@banan-it.com'),
(3,			N'Yisak Fikadu',		N'yisakfikadu79@gmail.com'),
(4,			N'Abrham Tenker',		N'abrham.tenker@banan-it.com'),
(5,			N'Wondewsen Semaneh',	N'wendylulu99@gmail.com'),
(6,			N'Abu Bakr elHadi',		N'abubaker.elhadi@banan-it.com');

DELETE FROM @Users WHERE [Email] IN (SELECT [Email] FROM dbo.Users); -- in case admin was in the list
EXEC [dal].[Users__Save]
	@Entities = @Users
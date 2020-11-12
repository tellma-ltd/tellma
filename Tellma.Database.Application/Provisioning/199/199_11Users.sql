INSERT INTO @Users
([Index],	[Name],					[Email]) VALUES
(0,			N'Jiad Akra',			N'jiad.akra@banan-it.com'),
(1,			N'Mohamad Akra',		N'mohamad.akra@banan-it.com'),
(2,			N'Ahmad Akra',			N'ahmad.akra@banan-it.com'),
(3,			N'Abu Bakr elHadi',		N'abubaker.elhadi@banan-it.com'),
(4,			N'Mosab elHafiz',		N'mosab.elhafiz@banan-it.com');

DELETE FROM @Users WHERE [Email] IN (SELECT [Email] FROM dbo.Users); -- in case admin was in the list
EXEC [dal].[Users__Save]
	@Entities = @Users

DECLARE @Partners dbo.[RelationList];
IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @Partners
	([Index], [Name]) VALUES
	(0,		N'Tom Hurton'),
	(1,		N'Jeff Bezos'),
	(2,		N'Warren Buffet');

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Partners
	([Index], [Name]) VALUES
	(0,		N'Sisay Tesfaye');

EXEC [api].[Relations__Save]
	@DefinitionId = @PartnerRLD,
	@Entities = @Partners,
	@UserId = @AdminUserId;
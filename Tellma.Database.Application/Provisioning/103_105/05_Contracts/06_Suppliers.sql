	DECLARE @Suppliers dbo.[RelationList];
	DELETE FROM @RelationUsers;
IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @Suppliers
	([Index], [Name],								[FromDate],	[TaxIdentificationNumber]) VALUES
	(0,		N'Banan Information technologies, plc',	'2017.09.15',	NULL),
	(1,		N'Regus',								'2018.01.05',	N'4544287'),
	(2,		N'Jimma Gas Station',					'2018.03.11',	NULL),
	(3,		N'Toyota',								'2019.03.19',	N'67675440'),
	(4,		N'Amazon',								'2019.05.09',	N'67075123');
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	INSERT INTO @Suppliers
	([Index], [Name],								[FromDate],	[TaxIdentificationNumber]) VALUES
	(0,		N'Banan Information technologies, plc',	'2017.09.15',	NULL),
	(1,		N'Yuangfan',							'2016.01.05',	NULL);
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Suppliers
	([Index], [Name],								[FromDate],	[TaxIdentificationNumber]) VALUES
	(0,		N'Banan Information technologies, plc',	'2017.09.15',	NULL),
	(1,		N'Noc Jimma Ber Service Station',		'2018.03.11',	NULL);
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	INSERT INTO @Suppliers
	([Index], [Name],					[Name2]) VALUES
	(0,		N'International Paper',		N'الورق العالمية'),
	(1,		N'Georgia-Pacific Corp',	N'جورجيا باسيفيك'),
	(2,		N'Weyerhaeuser Corporation',N'شركة ويرهاوزر'),
	(3,		N'Stora Enso',				N'ستورا إنسو'),
	(4,		N'Phoenix Pulp',			N'فونكس');

EXEC [api].[Relations__Save]
	@DefinitionId = @SupplierRLD,
	@Entities = @Suppliers,
	@RelationUsers = @RelationUsers,
	@UserId = @AdminUserId;

DECLARE @BananIT int, @Regus int, @NocJimma INT, @Toyota INT, @Amazon INT, @Stora INT, @Phoenix INT;
SELECT
	@BananIT = (SELECT [Id] FROM [dbo].[fi_Relations](N'suppliers', NULL) WHERE [Name] = N'Banan Information technologies, plc'),
	@Regus = (SELECT [Id] FROM [dbo].[fi_Relations](N'suppliers', NULL) WHERE [Name] = N'Regus'),
	@NocJimma = (SELECT [Id] FROM [dbo].[fi_Relations](N'suppliers', NULL) WHERE [Name] = N'Noc Jimma Ber Service Station'),
	@Toyota =  (SELECT [Id] FROM [dbo].[fi_Relations](N'suppliers', NULL) WHERE [Name] = N'Toyota, Ethiopia'),
	@Amazon =  (SELECT [Id] FROM [dbo].[fi_Relations](N'suppliers', NULL) WHERE [Name] = N'Amazon, Ethiopia'),
	@Stora =  (SELECT [Id] FROM [dbo].[fi_Relations](N'suppliers', NULL) WHERE [Name] = N'Stora Enso'),
	@Phoenix =  (SELECT [Id] FROM [dbo].[fi_Relations](N'suppliers', NULL) WHERE [Name] = N'Phoenix Pulp')
	;
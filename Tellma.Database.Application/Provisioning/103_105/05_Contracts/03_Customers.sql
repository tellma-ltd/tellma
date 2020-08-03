	DECLARE @Customers dbo.[RelationList];
	DECLARE @Paint int, @Plastic int, @WaliaSteel int, @Lifan int;

IF @DB = N'100' -- ACME, USD, en/ar/zh		
	INSERT INTO @Customers
	([Index],	[Name]) VALUES
	(0,			N'Customer1'),
	(1,			N'Customer2'),
	(2,			N'Customer3'),
	(3,			N'Customer4');


ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Customers
	([Index],	[Name],								[FromDate], [TaxIdentificationNumber]) VALUES
	(0,			N'3F Finfine Furniture Factory',	'2017.09.15', N'0000007551'),
	(1,			N'4 Good Management Consultant PLC','2017.10.25', N'0045782603'),
	(2,			N'A.M.M METAL',						'2018.01.05', N'0045000771'),
	(3,			N'A.R.C',							'2017.10.25', N'0023353621');

ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	INSERT INTO @Customers
	([Index],	[Name],						[Name2]) VALUES
	(0,			N'Riyadh Printshop',		N'مطبعة الرياض'),
	(1,			N'Pro Print',				N'الطباعة الاحترافية'),
	(2,			N'Zeej printing services',	N'خدمات زيج للطباعة'),
	(3,			N'Afan Printing & Packaging',N'آفان للطباعة والتغليف')	;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Customers: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	SELECT
		@WaliaSteel = (SELECT [Id] FROM [dbo].[fi_Relations](N'customers', NULL) WHERE [Name] = N'Walia Steel Industry, plc'),
		@Paint = (SELECT [Id] FROM [dbo].[fi_Relations](N'customers', NULL) WHERE [Name] = N'Best Paint Industry'),
		@Plastic = (SELECT [Id] FROM [dbo].[fi_Relations](N'customers', NULL) WHERE [Name] = N'Best Plastic Industry'),
		@Lifan = (SELECT [Id] FROM [dbo].[fi_Relations](N'customers', NULL) WHERE [Name] = N'Yangfan Motors, PLC');
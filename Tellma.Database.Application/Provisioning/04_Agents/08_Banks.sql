DECLARE @banks dbo.[AgentList];
DECLARE @Bank_CBE INT, @Bank_AWB INT, @Bank_NIB INT, @Bank_RJB INT;

IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @banks([Index],
		[Name],							[IsRelated], [Code]) VALUES
	(0, N'HSBC',						0,			'HSBC'),
	(1, N'Om Durman National Bank',		0,			'OMD');
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @banks([Index],
		[Name],							[IsRelated], [Code]) VALUES
	(0, N'Bank of Khartoum',			0,			'BOK'),
	(1, N'Om Durman National Bank',		0,			'OMD');
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @banks([Index],
		[Name],							[IsRelated], [Code]) VALUES
	(0, N'Commercial Bank of Ethiopia',	0,			'CBE'),
	(1, N'Awash Bank',					0,			'AWB'),
	(2, N'NIB',							0,			'NIB');
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	INSERT INTO @banks([Index],
		[Name],							[IsRelated], [Code]) VALUES
	(0, N'al-Rajihi Bank',				0,			'RJB');
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @banks([Index],
		[Name],							[IsRelated], [Code]) VALUES
	(0, N'Commercial Bank of Ethiopia',	0,			'CBE'),
	(1, N'Awash Bank',					0,			'AWB'),
	(2, N'NIB',							0,			'NIB');
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	INSERT INTO @banks([Index],
		[Name],								[Name2],						[Code]) VALUES
	(0, N'The National Commercial Bank',	N'البنك الأهلي التجاري',		'NCB'),
	(1, N'The Saudi British Bank',			N'البنك السعودي البريطاني',	'SBB'),
	(2, N'Saudi Investment Bank',			N'البنك السعودي للاستثمار',		'SIB'),
	(3, N'alinma bank',						N'مصرف الإنماء',					'NMB'),
	(4, N'Banque Saudi Fransi',				N'البنك السعودي الفرنسي',		'SFB'),
	(5, N'Riyad Bank',						N'بنك الرياض',					'RDB'),
	(6, N'alawwal bank',					N'البنك الأول',					'AWB'),
	(7, N'al Rajihi Bank',					N'مصرف الراجحي',				'RJB'),
	(8, N'Arab National Bank',				N'البنك العربي الوطني',		'ANB'),
	(9, N'Bank AlBilad',					N'بنك البلاد',					'BLB'),
	(10, N'Bank AlJazira',					N'بنك الجزيرة',					'JZB'),
	(11, N'Gulf International Bank Saudi Aribia (GIB-SA)',	N'بنك الخليج الدولي - السعودية','GIB')	
	;

	EXEC [api].[Agents__Save]
		@DefinitionId = N'banks',
		@Entities = @banks,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Banks: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

SELECT
	@Bank_CBE= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Commercial Bank of Ethiopia'),
	@Bank_AWB= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Awash Bank'),
	@Bank_NIB= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'NIB'),
	@Bank_RJB= (SELECT [Id] FROM [dbo].[Agents] WHERE [Code] = N'RJB');
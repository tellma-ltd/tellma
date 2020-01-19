DECLARE @banks dbo.[AgentList];

BEGIN -- Cleanup & Declarations
	DECLARE @Bank_CBE int, @Bank_AWB int,	@Bank_NIB int;
END
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
ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
	INSERT INTO @banks([Index],
		[Name],							[IsRelated], [Code]) VALUES
	(0, N'al-Rajihi Bank',				0,			'RJB');
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @banks([Index],
		[Name],							[IsRelated], [Code]) VALUES
	(0, N'Commercial Bank of Ethiopia',	0,			'CBE'),
	(1, N'Awash Bank',					0,			'AWB'),
	(2, N'NIB',							0,			'NIB');

	EXEC [api].[Agents__Save]
		@DefinitionId = N'banks',
		@Entities = @banks,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Banks: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	
	IF @DebugEmployees = 1
	SELECT A.[Code], A.[Name], A.[StartDate] AS 'Banking Since', A.[IsActive]
	--RC.[Name] AS OperatingSegment
	FROM dbo.fi_Agents(N'banks', NULL) A
	--LEFT JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;

SELECT
	@Bank_CBE= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Commercial Bank of Ethiopia'),
	@Bank_AWB= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Awash Bank'),
	@Bank_NIB= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'NIB');
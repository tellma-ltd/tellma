DECLARE @banks dbo.[AgentList];

BEGIN -- Cleanup & Declarations
	DECLARE @Bank_CBE int, @Bank_AWB int,	@Bank_NIB int;
END
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
		Print 'Banks: Inserting'
		GOTO Err_Label;
	END;
	
	IF @DebugEmployees = 1
	SELECT A.[Code], A.[Name], A.[StartDate] AS 'Banking Since', A.[IsActive],
	RC.[Name] AS OperatingSegment
	FROM dbo.fi_Agents(N'banks', NULL) A
	LEFT JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;

SELECT
	@Bank_CBE= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Commercial Bank of Ethiopia'),
	@Bank_AWB= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Awash Bank'),
	@Bank_NIB= (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'NIB');
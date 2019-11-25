BEGIN -- Cleanup & Declarations
	DECLARE @OperationsDTO dbo.[OperationList];
	DECLARE @WSI int, @Existing int, @Expansion int;

END
BEGIN -- Inserting
/*
WSI
	WSI - General
	Administration
		Administration - General
		Board
		Executive Office
		HR & General Services
		Finance
		Material management
		Quality and Safety
		MIS
	Sales and Marketing
		S&M - general
		S&M - Oromia - general
		S&M - Oromia - T/Haymanot
		S&M - Bole - general
		S&M - existing
		S&M - expansion
	Production
		Production - General
		Production - existing
		Production - expansion
	Technical
		Technical - general
		Technical - existing
		Technical - expansion
	Other Operations
		T/Haymanot
		Global Building
		Coffee processing	
		Kersa
		Walia Water Bottling
*/
	INSERT INTO @OperationsDTO
		([Name],				[ParentIndex]) Values
		(N'Walia Steel Industry', NULL), -- 0
			(N'Administration', 0), -- 1
				(N'Board', 1),
				(N'Executive Office', 1),
				(N'HR & General Services', 1),
				(N'Finance', 1),
				(N'Material management', 1),
				(N'Quality and Safety', 1),
				(N'Technical', 1),
				(N'MIS', 1), -- 9
			(N'Sales and Marketing', 0), -- 10	
				(N'Sales and Marketing Overhead', 10),			
				(N'Oromia', 10),
				(N'Bole', 10),
			(N'Production', 0), -- 14
				(N'Production Overhead', 14),		
				(N'Existin', 14),
				(N'Expansion', 14),
			(N'Other Operations', 0), -- 18
				(N'T/Haymanot', 18),
				(N'Global Building', 18),
				(N'Coffee processing', 18),
				(N'Kersa', 18),
				(N'Fake', 18),
				(N'Walia Water Bottling', 18),
		(N'New Kersa', NULL);

	EXEC [api].[ResponsibilityCenters__Save]
		@Entities = @OperationsDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ResultsJson = @ResultsJson OUTPUT

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Place: Operations 1'
		GOTO Err_Label;
	END

	IF @DebugOperations = 1
		SELECT * FROM [dbo].[fr_ResponsibilityCenters__Json](@ResultsJson)
END
BEGIN
	DELETE FROM @OperationsDTO;
	INSERT INTO @OperationsDTO (
		[Id], [Name], [ParentId], [Code]
	)
	SELECT
		[Id], [Name], [ParentId], [Code]
	FROM [dbo].[ResponsibilityCenters];

	UPDATE @OperationsDTO 
	SET 
		[Name] = N'Existing',
		[EntityState] = N'Updated'
	WHERE [Name] = N'Existin';

	UPDATE @OperationsDTO 
	SET 
		[EntityState] = N'Deleted'
	WHERE [Name] = N'Fake';

	EXEC [api].[ResponsibilityCenters__Save]
		@Entities = @OperationsDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ResultsJson = @ResultsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Place: Operations 2'
		GOTO Err_Label;
	END
	IF @DebugOperations = 1
		SELECT * FROM [dbo].[fr_ResponsibilityCenters__Json](@ResultsJson);
END
SELECT
	@WSI = (SELECT [Id] FROM [dbo].[ResponsibilityCenters] WHERE [Name] = N'Walia Steel Industry'),
	@Existing = (SELECT [Id] FROM [dbo].[ResponsibilityCenters] WHERE [Name] = N'Existing'),
	@Expansion = (SELECT [Id] FROM [dbo].[ResponsibilityCenters] WHERE [Name] = N'Expansion');
	
EXEC api_Operation__SetOperatingSegment
	@OperationId = @WSI,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@ResultsJson = @ResultsJson OUTPUT;
	IF @DebugOperations = 1
		SELECT * FROM [dbo].[fr_ResponsibilityCenters__Json](@ResultsJson);

EXEC api_Operation__SetOperatingSegment
	@OperationId = @Existing,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@ResultsJson = @ResultsJson OUTPUT;
	IF @DebugOperations = 1
		SELECT * FROM [dbo].[fr_ResponsibilityCenters__Json](@ResultsJson);

EXEC api_Operation__SetOperatingSegment
	@OperationId = @WSI,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
	@ResultsJson = @ResultsJson OUTPUT;
	IF @DebugOperations = 1
		SELECT * FROM [dbo].[fr_ResponsibilityCenters__Json](@ResultsJson);
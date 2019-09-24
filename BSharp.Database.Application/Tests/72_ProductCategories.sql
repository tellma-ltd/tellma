BEGIN -- Cleanup & Declarations
	DECLARE @ProductCategoriesDTO [dbo].ProductCategoryList;
END
BEGIN -- Inserting
	INSERT INTO @ProductCategoriesDTO (
	[Index], [Name],					[ParentIndex], [Code]) VALUES
	(1, N'Hollow Section Product',		NULL,			N'1'),
	(2, N'Circular Hollow Section',		1,				N'11'),
	(3, N'Rectangular Hollow Section',	1,				N'12'),
	(4, N'Square Hollow Section',		1,				N'13'),
	(6, N'LTZ Products',				NULL,			N'2'),
	(7, N'L Bars',						6,				N'21'),
	(8, N'T Bars',						6,				N'22'),
	(9, N'Z Bars',						6,				N'23'),
	(10, N'Sheet Metals',				NULL,			N'3')
	;

	EXEC [api].[ResourceClassifications__Save]
		@Entities = @ProductCategoriesDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ReturnIds = 1;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'ProductCategories: Place 1'
		GOTO Err_Label;
	END

	IF @DebugProductCategories = 1
		EXEC [api].[ResourceClassifications__Select];

	DELETE FROM @ProductCategoriesDTO;
	INSERT INTO @ProductCategoriesDTO([Id], [Index], [Name], [ParentId], [ParentIndex], [Code], [EntityState])
	SELECT [Id], [Id], [Name], [ParentId], NULL, [Code], N'Unchanged' FROM dbo.[ResourceClassifications];

	UPDATE @ProductCategoriesDTO SET [Name] = N'L-T-Z Products', [EntityState] = N'Updated' WHERE [Code] = N'2';
	
	DECLARE @MaxIndex INT = (SELECT MAX([Index]) FROM @ProductCategoriesDTO);
	INSERT INTO @ProductCategoriesDTO (
	[Index], [Name],				[ParentIndex], [ParentId], [Code]) VALUES
	(@MaxIndex + 1, N'D Hollow Section',NULL,(SELECT [Id] FROM @ProductCategoriesDTO WHERE [Code] = '1'),		N'14'),
	(@MaxIndex + 2, N'Slittes Sheet Metal',NULL,	(SELECT [Id] FROM @ProductCategoriesDTO WHERE [Code] = '3'),	N'31');
	-- If we merge with the previous statement, [ParentIndex] would come out NULL.
	INSERT INTO @ProductCategoriesDTO (
	[Index], [Name],				[ParentIndex], [ParentId], [Code]) VALUES
	(@MaxIndex + 3, N'Checkered Slitted Sheet Metal',(SELECT [Index] FROM @ProductCategoriesDTO WHERE [Code] = '31'), NULL,	N'312');
	
	EXEC [api].[ResourceClassifications__Save]
		@Entities = @ProductCategoriesDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ReturnIds = 1;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'ProductCategories: Place 2'
		GOTO Err_Label;
	END

	IF @DebugProductCategories = 1
		EXEC [api].[ResourceClassifications__Select];
END

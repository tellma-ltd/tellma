
DECLARE @R4 [dbo].ResourceList, @RP4 [dbo].ResourceInstanceList;

INSERT INTO @R4 ([Index],
		[Name],				[VolumeUnitId]) VALUES
	(0, N'Oil',		@LiterUnit),
	(1, N'Diesel',	@LiterUnit);

	EXEC [api].[Resources__Save] -- N'vehicles'
		@DefinitionId = N'production-supplies',
		@Resources = @R4,
	--	@Instances = @RP4,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting production supplies'
		GOTO Err_Label;
	END;
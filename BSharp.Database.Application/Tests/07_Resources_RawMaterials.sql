DECLARE @R3 [dbo].ResourceList, @RP3 [dbo].ResourcePickList;

	INSERT INTO @R3 ([Index],
		[Name],				[Code],			[MassUnitId], [CountUnitId]) VALUES
	(0,	N'HR 1000MMx1.9MM',	N'HR1000x1.9',	@KgUnit, @pcsUnit),
	(1,	N'CR 1000MMx1.4MM',	N'CR1000x1.4',	@KgUnit, @pcsUnit);
	INSERT INTO @RP3([Index], [ResourceIndex],
	[ProductionDate],	[Code],		[Mass]) VALUES
	(4,0,N'2017.10.01',	N'54001',	7891),
	(5,0,N'2017.10.15',	N'54002',	6985),
	(6,0,N'2017.10.15',	N'60032',	7320),
	(7,0,N'2017.10.01',	N'60342',	7100);
	EXEC [api].[Resources__Save] --  N'raw-materials'
		@ResourceDefinitionId = N'raw-materials',
		@Resources = @R3,
		@Picks = @RP3,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting raw materials'
		GOTO Err_Label;
	END;
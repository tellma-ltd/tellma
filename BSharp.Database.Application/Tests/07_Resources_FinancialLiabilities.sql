	DECLARE @R7 [dbo].ResourceList,  @RP7 [dbo].ResourcePickList;

	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'financial-liabilities
								[Name],									[IsLeaf],	[Node]) VALUES
	(N'financial-liabilities',	N'at fair value through profit or loss',	1,			N'/1/'),
	(N'financial-liabilities',	N'at amortised cost',						1,			N'/2/'),
	--(N'financial-liabilities',	N'Shares (issued)',							1,			N'/2/1/'),
	(N'financial-liabilities',	N'Bonds (issued)',							1,			N'/2/1/'),
	(N'financial-liabilities',	N'L/G (issued)',							1,			N'/2/2/'),
	(N'financial-liabilities',	N'Notes (issued)',							1,			N'/2/3/')
	;
	 --trade payables, loans from other entities, and debt instruments issued by the entity.
	
	-- issued shares are actually equity instruments, since there is no obligation to repay
	DECLARE @RCFL INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceDefinitionId] = N'financial-liabilities' AND [Node] = N'/4/');
	INSERT INTO @R7 ([Index], [ResourceClassificationId],
				[Name],				[Code],			[CountUnitId]) VALUES
	(0, @RCFL,	N'Common Stock',	N'CMNSTCK',		@shareUnit),
	(1, @RCFL,	N'Premium Stock',	N'PRMMSTCK',	@shareUnit);
	EXEC [api].[Resources__Save] -- N'financial-liabilities'
		@ResourceDefinitionId = N'financial-liabilities',
		@Resources = @R7,
		@Picks = @RP7,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting financial liabilities'
		GOTO Err_Label;
	END;
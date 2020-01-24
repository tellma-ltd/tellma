	DECLARE @R7 [dbo].ResourceList,  @RP7 [dbo].ResourceInstanceList;

	INSERT INTO dbo.[AccountTypes] (-- N'financial-liabilities
								[Name],									[IsLeaf],	[Node]) VALUES
	(N'at fair value through profit or loss',	1,			N'/1/'),
	(N'at amortised cost',						1,			N'/2/'),
	--(N'Shares (issued)',							1,			N'/2/1/'),
	(N'Bonds (issued)',							1,			N'/2/1/'),
	(N'L/G (issued)',							1,			N'/2/2/'),
	(N'Notes (issued)',							1,			N'/2/3/')
	;
	 --trade payables, loans from other entities, and debt instruments issued by the entity.
	
	-- issued shares are actually equity instruments, since there is no obligation to repay
	DECLARE @RCFL INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [ResourceDefinitionId] = N'financial-liabilities' AND [Node] = N'/4/');
	INSERT INTO @R7 ([Index], [AccountTypeId],
				[Name],				[Code],			[CountUnitId], [CurrencyId]) VALUES
	(0, @RCFL,	N'Common Stock',	N'CMNSTCK',		@shareUnit, N'USD'),
	(1, @RCFL,	N'Premium Stock',	N'PRMMSTCK',	@shareUnit, N'USD');
	EXEC [api].[Resources__Save] -- N'financial-liabilities'
		@DefinitionId = N'financial-liabilities',
		@Resources = @R7,
	--	@Instances = @RP7,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting financial liabilities'
		GOTO Err_Label;
	END;
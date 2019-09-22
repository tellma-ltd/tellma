	DECLARE @R8 [dbo].ResourceList,  @RP8 [dbo].ResourcePickList;

	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'financial-liabilities
								[Name],														[IsLeaf],	[Node]) VALUES
	(N'employee-benefits',	N'Short-term employee benefits expense',						0,			N'/1/'),
	(N'employee-benefits',	N'Wages and salaries',											0,			N'/1/1/'),
	(N'employee-benefits',	N'Social security contributions',								0,			N'/1/2/'),
	(N'employee-benefits',	N'Other short-term employee benefits',							0,			N'/1/3/'),
	(N'employee-benefits',	N'Post-employment benefit expense, defined contribution plans',	1,			N'/2/'),
	(N'employee-benefits',	N'Post-employment benefit expense, defined benefit plans',		1,			N'/3/'),
	(N'employee-benefits',	N'Termination benefits expense',								1,			N'/4/'),
	(N'employee-benefits',	N'Other long-term employee benefits',							1,			N'/5/')
	;--Other employee expense
	 --trade payables, loans from other entities, and debt instruments issued by the entity.
	
	-- issued shares are actually equity instruments, since there is no obligation to repay
	DECLARE @RCWS INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceDefinitionId] = N'employee-benefits' AND [Node] = N'/1/1/');
	INSERT INTO @R8 ([Index], [ResourceClassificationId],
				[Name],				[TimeUnitId]) VALUES
	(0, @RCWS,	N'Basic',			NULL),
	(1, @RCWS,	N'Transportation',	NULL),
	(2, @RCWS,	N'Holiday Overtime',@hrUnit),
	(3, @RCWS,	N'Rest Overtime',	@hrUnit),
	(4, @RCWS,	N'Labor (hourly)',	@hrUnit),
	(5, @RCWS,	N'Labor (daily)',	@dayUnit)
	;
	EXEC [api].[Resources__Save] -- N'employee-benefits'
		@DefinitionId = N'employee-benefits',
		@Resources = @R8,
	--	@Picks = @RP8,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting employee benefits'
		GOTO Err_Label;
	END;
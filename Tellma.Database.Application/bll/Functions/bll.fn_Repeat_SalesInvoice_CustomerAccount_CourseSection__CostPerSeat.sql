CREATE FUNCTION bll.fn_Repeat_SalesInvoice_CustomerAccount_CourseSection__CostPerSeat(@Repeat BIT, @SalesInvoice INT, @CustomerAccount INT, @CourseSectionId INT)
RETURNS DECIMAL (19,6)
AS
BEGIN
	DECLARE @CostPerSeat DECIMAL (19, 6);
	DECLARE @NoInvoice INT = dal.fn_AgentDefinition_Code__Id(N'SalesInvoice', N'0');
	SET @SalesInvoice = ISNULL(@SalesInvoice, @NoInvoice);
	DECLARE @InvoicedPaid BIT = IIF(@SalesInvoice = @NoInvoice, 0, 1);
	DECLARE @SeatUnitId INT = dal.fn_UnitCode__Id(N'Seat');
	DECLARE @CourseId INT =  dal.fn_Resource__Resource1Id(@CourseSectionId);
	DECLARE @CourseSectionLastDate DATE = dal.fn_Resource__ToDate(@CourseSectionId);
	DECLARE @Today DATE = GETDATE();
	DECLARE @PostingDate DATE = IIF (@CourseSectionLastDate < @Today, @CourseSectionLastDate, @Today);
	DECLARE @AbstractCustomer INT = dal.fn_AgentDefinition_Code__Id(N'TradeReceivableAccount', N'0');

	IF @Repeat = 1
		SELECT @CostPerSeat = 0;
	ELSE BEGIN
		IF @InvoicedPaid = 0 -- read from contract
		BEGIN
			SELECT @CostPerSeat = [NotedAmount] * dal.fn_Unit__BaseAmount(@SeatUnitId) / 
						dal.fn_Unit__BaseAmount([UnitId])
				FROM dbo.Entries E
				JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
				JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
				JOIN dbo.Lines L ON L.[Id] = E.[LineId]
				JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	
				WHERE E.[Index] = 0
				AND AC.[Concept] = N'CRMExtension'
				AND LD.[LineType] = 80
				AND L.[State] = 2
				AND E.[AgentId] = @CustomerAccount 
				AND E.[ResourceId] = @CourseId
				AND E.[Time1] <= @PostingDate
				AND (E.[Time2] IS NULL OR E.[Time2] >= @PostingDate)
			IF @CostPerSeat  IS NULL
			SELECT @CostPerSeat =  [NotedAmount] * dal.fn_Unit__BaseAmount(@SeatUnitId) / 
						dal.fn_Unit__BaseAmount([UnitId])
				FROM dbo.Entries E
				JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
				JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
				JOIN dbo.Lines L ON L.[Id] = E.[LineId]
				JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
				WHERE E.[Index] = 0
				AND AC.[Concept] = N'CRMExtension'
				AND LD.[LineType] = 80
				AND L.[State] = 2
				AND E.[AgentId] = @AbstractCustomer
				AND E.[ResourceId] = @CourseId
				AND E.[Time1] <= @PostingDate
				AND (E.[Time2] IS NULL OR E.[Time2] >= @PostingDate)
		END
		ELSE BEGIN-- read from sales invoice
			SELECT @CostPerSeat = L.[Decimal1] -- [NotedAmount] * dal.fn_Unit__BaseAmount(@SeatUnitId) / dal.fn_Unit__BaseAmount([UnitId])
				FROM dbo.Entries E
				JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
				JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
				JOIN dbo.Lines L ON L.[Id] = E.[LineId]
				JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
				WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
				AND LD.[LineType] = 100
				AND L.[State] = 4
				AND E.[NotedAgentId] = @SalesInvoice 
				AND E.[NotedResourceId] = @CourseId
		END
	END;
	RETURN @CostPerSeat;
END
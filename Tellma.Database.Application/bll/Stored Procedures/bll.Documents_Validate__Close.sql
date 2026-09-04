CREATE PROCEDURE [bll].[Documents_Validate__Close]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @Documents [dbo].[DocumentList], @DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList],
			@Lines [dbo].[LineList], @Entries [dbo].[EntryList];
	DECLARE @ManualJV INT = (SELECT [Id] FROM dbo.DocumentDefinitions WHERE [Code] = N'ManualJournalVoucher');
	SET @IsError = 0;
	DECLARE @EndOfLine NVARCHAR(5) = ',' + CHAR(13) + CHAR(10);
	-- Fill vaidation error list like the others
	DECLARE @Err NVARCHAR(MAX)
	SELECT @Err = STRING_AGG(Err, @EndOfLine) 
	FROM (
		SELECT
		N'Tab: ' + LD.titlesingular +
		N', dbo.Line #: '+ CAST (e.[index] + 1 as nvarchar (50)) +
		N', Account: '+ A.[Name] +
		N', Amount: '+ FORMAT(e.MonetaryValue, N'N2') +
		N', Value: '+ FORMAT(e.[Value], N'N2') +
		N'. Resource: '+ R.[Name] + ' is wrong' AS Err
		FROM entries e
		JOIN dbo.Lines l ON L.id = e.Lineid
		JOIN dbo.LineDefinitions LD ON LD.id = L.definitionid
		JOIN dbo.Accounts A ON A.id = e.accountid
		JOIN dbo.AccountTypes ac ON ac.id = A.accounttypeid
		JOIN dbo.Resources r ON R.id = e.resourceid
		LEFT JOIN AccountTypeResourceDefinitions ACRD ON ACRD.AccountTypeId = ac.id and ACRD.ResourceDefinitionId = R.DefinitionId
		WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
		AND ACRD.id IS NULL
		AND L.[State] >= 0

		UNION

		SELECT
		N'Tab: ' + LD.titlesingular +
		N', dbo.Line #: '+ CAST (e.[index] + 1 as nvarchar (50)) +
		N', Account: '+ A.[Name] +
		N', Amount: '+ FORMAT(e.MonetaryValue, N'N2') +
		N', Value: '+ FORMAT(e.[Value], N'N2') +
		N'. Noted Resource: '+ R.[Name] + ' is wrong'
		FROM entries e
		JOIN dbo.Lines l ON L.id = e.Lineid
		JOIN dbo.LineDefinitions LD ON LD.id = L.definitionid
		JOIN dbo.Accounts A ON A.id = e.accountid
		JOIN dbo.AccountTypes ac ON ac.id = A.accounttypeid
		JOIN dbo.Resources r ON R.id = e.NotedResourceId
		LEFT JOIN AccountTypeNotedResourceDefinitions ACRD ON ACRD.AccountTypeId = ac.id and ACRD.NotedResourceDefinitionId = R.DefinitionId
		WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
		AND ACRD.id IS NULL
		AND L.[State] >= 0

		UNION

		SELECT
		N'Tab: ' + LD.titlesingular +
		N', dbo.Line #: '+ CAST (e.[index] + 1 as nvarchar (50)) +
		N', Account: '+ A.[Name] +
		N', Amount: '+ FORMAT(e.MonetaryValue, N'N2') +
		N', Value: '+ FORMAT(e.[Value], N'N2') +
		N'. Agent: '+ AG.[Name] + ' is wrong'
		FROM entries e
		JOIN dbo.Lines l ON L.id = e.Lineid
		JOIN dbo.LineDefinitions LD ON LD.id = L.definitionid
		JOIN dbo.Accounts A ON A.id = e.accountid
		JOIN dbo.AccountTypes ac ON ac.id = A.accounttypeid
		JOIN agents AG ON AG.id = e.agentid
		LEFT JOIN AccountTypeagentDefinitions ACRD ON ACRD.AccountTypeId = ac.id and ACRD.agentDefinitionId = AG.DefinitionId
		WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
		AND ACRD.id IS NULL
		AND L.[State] >= 0

		UNION

		SELECT
		N'Tab: ' + LD.titlesingular +
		N', dbo.Line #: '+ CAST (e.[index] + 1 as nvarchar (50)) +
		N', Account: '+ A.[Name] +
		N', Amount: '+ FORMAT(e.MonetaryValue, N'N2') +
		N', Value: '+ FORMAT(e.[Value], N'N2') +
		N'. Noted Agent: '+ AG.[Name] + ' is wrong'
		FROM entries e
		JOIN dbo.Lines l ON L.id = e.Lineid
		JOIN dbo.LineDefinitions LD ON LD.id = L.definitionid
		JOIN dbo.Accounts A ON A.id = e.accountid
		JOIN dbo.AccountTypes ac ON ac.id = A.accounttypeid
		JOIN agents AG ON AG.id = e.NotedagentId
		LEFT JOIN AccountTypeNotedagentDefinitions ACRD ON ACRD.AccountTypeId = ac.id and ACRD.NotedagentDefinitionId = AG.DefinitionId
		WHERE L.[DocumentId] IN (SELECT [Id] FROM @Ids)
		AND ACRD.id IS NULL
		AND L.[State] >= 0
	) T

	IF @Err IS NOT NULL
		THROW 50000, @Err, 1;

	-- cannot close if the line posting date falls in an archived period. Logic repeated at line level
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_FallsinArchivedPeriod', NULL
	FROM @Ids FE
	JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	WHERE L.[PostingDate] <= (SELECT [ArchiveDate] FROM dbo.Settings)
	AND LD.[LineType] >= 100
	--UNION
	--SELECT DISTINCT TOP (@Top)
	--	'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
	--	N'Error_FallsinFrozenPeriod', NULL
	--FROM @Ids FE
	--JOIN dbo.Documents D ON FE.[Id] = D.[Id]
	--JOIN dbo.Lines L ON L.[DocumentId] = D.[Id]
	--JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	--WHERE L.[PostingDate] <= (SELECT [FreezeDate] FROM dbo.Settings)
	--AND LD.[LineType] >= 100
	UNION
	-- Cannot close it if it is not draft
	--INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentIsNotInState0',
		N'localize:Document_State_0'
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 0
	UNION
	-- Cannot close it if it has no attachments while attachments are required
	--INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_DocumentHasNoAttachment', NULL
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	JOIN [dbo].[DocumentDefinitions]  DD ON D.[DefinitionId] = DD.[Id]
	LEFT JOIN [dbo].[Attachments] A ON D.[Id] = A.[DocumentId]
	WHERE DD.[AttachmentVisibility] = N'Required'
	AND A.[Id] IS NULL;

	-- Cannot close a document where there are no lines, or where all lines have negative state
	-- So, we take all documents and remove from them those with positive states
	WITH NonSatisfactoryDocuments AS (
		SELECT [Index]
		FROM @Ids
		EXCEPT (
			SELECT DISTINCT FE.[Index]
			FROM @Ids FE
			JOIN [dbo].[Lines] L ON L.[DocumentId] = FE.[Id]
			JOIN [map].[LineDefinitions]() LD ON L.[DefinitionId] = LD.[Id]
			WHERE
				LD.[HasWorkflow] = 1 AND L.[State]  = LD.[LastLineState]
			OR	LD.[HasWorkflow] = 0
		)
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top) 
		'[' + CAST([Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentDoesNotHaveAnyPostedLines'
	FROM @Ids
	WHERE [Index] IN (SELECT [Index] FROM NonSatisfactoryDocuments);

	-- Cannot close a document which has lines with missing signatures
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasLinesWithMissingSignatures'
	FROM @Ids FE
	JOIN [dbo].[Lines] L ON L.[DocumentId] = FE.[Id]
	JOIN [map].[LineDefinitions]() LD ON LD.[Id] = L.[DefinitionId]
	WHERE LD.[HasWorkflow] = 1 AND L.[State] BETWEEN 0 AND LD.[LastLineState] - 1;

	-- To do: cannot close a document with a control account having non zero balance
	IF (@DefinitionId <> @ManualJV)
	AND EXISTS (
		SELECT * FROM
		dbo.DocumentDefinitionLineDefinitions DDLD
		JOIN dbo.LineDefinitions LD ON LD.[Id] = DDLD.[LineDefinitionId]
		WHERE DDLD.[DocumentDefinitionId] = @DefinitionId
		AND LD.[LineType] >= 100 -- N'Event', N'Regulatory'
	)
	WITH ControlAccountTypes AS (
		SELECT [Id]
		FROM [dbo].[AccountTypes]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM [dbo].[AccountTypes] WHERE [Concept] = N'ControlAccountsExtension')
		) = 1
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasControlAccount0For1WithNetBalance2' AS [ErrorName],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) As AccountName,
		[dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) AS NotedAgent,
		FORMAT(SUM(E.[Direction] * E.[MonetaryValue]), 'G', 'en-us') AS NetBalance
	FROM @Ids D
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[LineDefinitions] LD ON LD.[Id] = L.[DefinitionId]
	JOIN [dbo].[Entries] E ON E.[LineId] = L.[Id]
	JOIN [dbo].[Accounts] A ON E.[AccountId] = A.[Id]
	-- MA: LEFT JOIN => JOIN, assuming control accounts have Noted Agent. 2021.12.11
	JOIN [dbo].[Agents] R ON E.[NotedAgentId] = R.[Id]
	WHERE A.AccountTypeId IN (SELECT [Id] FROM ControlAccountTypes)
	AND LD.[LineType] >= 100 -- N'Event', N'Regulatory'
	AND L.[State] >= 0 -- to cater for both Draft in workflow-less and for posted.
	GROUP BY D.[Index], [dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]), E.[CurrencyId], E.[CenterId], [dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) 
	HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	UNION
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(D.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentHasControlAccount0For1WithNetBalance2' AS [ErrorName],
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) As AccountName,
		dbo.fn_Localize(R.[Name], R.[Name2], R.[Name3]) AS NotedAgent,
		FORMAT(SUM(E.[Direction] * E.[Value]), 'G', 'en-us') AS NetBalance
	FROM @Ids D
	JOIN [dbo].[Lines] L ON L.[DocumentId] = D.[Id]
	JOIN [dbo].[LineDefinitions] LD ON LD.[Id] = L.[DefinitionId]
	JOIN dbo.Entries E ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	-- MA: LEFT JOIN => JOIN, assuming control accounts have Noted Agent. 2021.12.11
	JOIN [dbo].[Agents] R ON E.[NotedAgentId] = R.[Id]
	WHERE A.AccountTypeId IN (SELECT [Id] FROM ControlAccountTypes)
	AND LD.[LineType] >= 100
	AND L.[State] >= 0 -- to cater for both Draft in workflow-less and for posted.
	-- MA: removed CurrencyId From GROUP BY, 2021.12.11
	GROUP BY D.[Index], [dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]), E.[CenterId], [dbo].[fn_Localize](R.[Name], R.[Name2], R.[Name3]) 
	HAVING SUM(E.[Direction] * E.[Value]) <> 0

	-- cannot close a document with sales invoice, if it violates one of the following
	DECLARE @Country NCHAR (2) = dal.fn_Settings__Country();
	IF @Country = N'SA' AND @DefinitionId <> @ManualJV
	AND EXISTS(
		SELECT *
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN @Ids D ON D.[Id] = L.[DocumentId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
	)

	-- If there are ZATCA documents, assert that all ZATCA rules are observed
	IF [dal].[fn_DocumentDefinition__IsZatcaDocumentType](@DefinitionId) = 1
	BEGIN
		INSERT INTO @ValidationErrors([Key], [ErrorName])
		-- Missing invoice type transaction
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheDocumentHasMissingInvoiceTypeTransaction'
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE D.[Lookup1Id] IS NULL
		UNION
		-- Missing invoice
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheDocumentHasMissingInvoice'
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE D.[NotedAgentId] IS NULL
		UNION
		-- Missing invoice currency
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheInvoiceHasMissingCurrency'
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents NAG ON NAG.[Id] = D.[NotedAgentId]
		WHERE NAG.[CurrencyId] IS NULL
		UNION
		-- Wrong date
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheDocumentPostingDateMustBeToday'
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE D.[PostingDate] <> CAST(GETDATE() AS DATE) 
	END
	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE;

	-- If this is a Marmin (UAE) document definition, assert everything the vendor requires is
	-- present BEFORE the close, because a close is the point of no return: after it the document
	-- is on the Peppol network and cannot be reopened.
	IF (SELECT [MarminAeDocumentType] FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId) IS NOT NULL
	BEGIN
		DECLARE @MarminAeIsCreditNote BIT = IIF(
			(SELECT [MarminAeDocumentType] FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId) = N'SalesCreditNote', 1, 0);

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		-- The vendor requires an email on the customer party, and Tellma allows it to be blank.
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_MarminAeCustomerHasNoEmail',
			CA.[Name]
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]
		JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id]
		LEFT JOIN dbo.Agents CG ON CG.[Id] = CA.[Agent1Id]
		WHERE ISNULL(CA.[ContactEmail], CG.[ContactEmail]) IS NULL

		UNION
		-- The tax registration number IS the Peppol routing address, so without it the document
		-- cannot be addressed at all. Unlike ZATCA there is no commercial-registration fallback.
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_MarminAeCustomerHasNoTaxId',
			CA.[Name]
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]
		JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id]
		LEFT JOIN dbo.Agents CG ON CG.[Id] = CA.[Agent1Id]
		WHERE ISNULL(CA.[TaxIdentificationNumber], CG.[TaxIdentificationNumber]) IS NULL

		UNION
		-- country_subentity must be an emirate CODE. Tellma stores AddressProvince as free text,
		-- so a city or a spelled-out emirate name would be rejected by the vendor at submission.
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_MarminAeInvalidEmirateCode',
			ISNULL(CA.[AddressProvince], N'')
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]
		JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id]
		WHERE dal.fn_Lookup__Code(CA.[AddressCountryId]) = N'AE'
		-- The vendor's own Schematron asserts exactly this set, and it is what
		-- GET /api/codelist/uae-subdivisions returns. They are three-letter codes
		-- (DXB, AUH, ...), not the two-letter ISO 3166-2:AE subdivisions.
		AND ISNULL(CA.[AddressProvince], N'') NOT IN (N'AUH', N'DXB', N'SHJ', N'UAQ', N'FUJ', N'AJM', N'RAK')

		UNION
		-- unit_code must be a UN/ECE Recommendation 20 code. It is free text in Tellma, so the
		-- most we can assert here is that there is one at all.
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_MarminAeInvalidUnitCode',
			NR.[Name]
		FROM @Ids FE
		JOIN [map].[Lines]() L ON L.[DocumentId] = FE.[Id]
		JOIN dbo.Entries E ON E.[LineId] = L.[Id]
		JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		LEFT JOIN dbo.Units U ON U.[Id] = E.[UnitId]
		WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
		AND ISNULL(U.[Code], N'') = N''

		UNION
		-- A credit note must name exactly one original invoice. Zero leaves billing_reference
		-- empty, which the vendor rejects; more than one means the NotedAgentId heuristic cannot
		-- tell which invoice is being adjusted, and guessing would attach the credit to the wrong
		-- one. Either way this is a data problem to fix before the note goes out.
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_MarminAeNoOriginalInvoice',
			CAST(D.[Id] AS NVARCHAR (255))
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE @MarminAeIsCreditNote = 1
		AND (
			SELECT COUNT(*)
			FROM dbo.Documents O
			JOIN dbo.DocumentDefinitions ODD ON ODD.[Id] = O.[DefinitionId]
			WHERE ODD.[MarminAeDocumentType] = N'SalesInvoice'
			AND O.[State] = 1
			AND O.[MarminAeState] >= 1
			AND O.[NotedAgentId] = D.[NotedAgentId]
		) <> 1

		UNION
		-- The reconciliation check, and the highest-value assertion here.
		--
		-- Marmin computes every total server-side from the lines we send, so a line-mapping error
		-- does not fail loudly: it produces a legally transmitted invoice whose total differs from
		-- the ledger, discovered later by the customer's counterparty. Recomputing the VAT exactly
		-- the way dal.MarminAe__GetInvoices will emit it, and comparing against what the ledger
		-- says, catches that here -- while the close can still be refused.
		--
		-- The tolerance absorbs per-line rounding only; a real mapping error (an unmodelled
		-- discount, a missed line) is far larger than a cent.
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_MarminAeTotalsMismatch',
			CAST(D.[Id] AS NVARCHAR (255))
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		CROSS APPLY (
			SELECT SUM(ROUND(
				(IIF(@MarminAeIsCreditNote = 1, +1, -1) * E.[Direction] * E.[Quantity])
					* L.[Decimal1] * ISNULL(NR.[VatRate], 0.05), 2)) AS [Vat]
			FROM [map].[Lines]() L
			JOIN dbo.Entries E ON E.[LineId] = L.[Id]
			JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
			JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = NR.[DefinitionId]
			JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
			JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
			WHERE L.[DocumentId] = D.[Id]
			AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
			AND NOT (NRD.[Code] = N'Discounts' OR NR.[Code] = N'RetentionByCustomer'
				OR NRD.[Code] LIKE N'Prepayments%' AND E.[Direction] = 1)
		) MAPPED
		WHERE ABS(ISNULL(MAPPED.[Vat], 0)
			- ABS(dal.fn_Document__InvoiceTotalVatAmountInAccountingCurrency(D.[Id]))) > 0.02
	END
	IF EXISTS(SELECT * FROM @ValidationErrors) GOTO DONE;
	-- Verify that workflow-less lines in Documents can be in their final state
	INSERT INTO @Documents ([Index], [Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]	
	)
	SELECT Ids.[Index], D.[Id], [SerialNumber], [Clearance], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	FROM [dbo].[Documents] D JOIN @Ids Ids ON D.[Id] = Ids.[Id]

	INSERT INTO @DocumentLineDefinitionEntries(
		[Index], [DocumentIndex], [Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	)
	SELECT 	DLDE.[Id], Ids.[Index], DLDE.[Id], [LineDefinitionId], [EntryIndex], [PostingDate], [PostingDateIsCommon], [Memo], [MemoIsCommon],
		[CurrencyId], [CurrencyIsCommon], [CenterId], [CenterIsCommon], [AgentId], [AgentIsCommon], [NotedAgentId], [NotedAgentIsCommon], 
		[ResourceId], [ResourceIsCommon], [NotedResourceId], [NotedResourceIsCommon], [Quantity], [QuantityIsCommon], [UnitId], [UnitIsCommon],
		[Time1], [Time1IsCommon], [Duration], [DurationIsCommon], [DurationUnitId], [DurationUnitIsCommon], [Time2], [Time2IsCommon],
		[NotedDate], [NotedDateIsCommon], [ExternalReference], [ExternalReferenceIsCommon], [ReferenceSourceId], [ReferenceSourceIsCommon],
		[InternalReference], [InternalReferenceIsCommon]
	FROM DocumentLineDefinitionEntries DLDE
	JOIN @Ids Ids ON DLDE.[DocumentId] = Ids.[Id]
	AND [LineDefinitionId]  IN (SELECT [Id] FROM [map].[LineDefinitions]() WHERE [HasWorkflow] = 0);

	-- Verify that lines whose last state = approved meet the conditions to be approved
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
			[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
			L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
	FROM [dbo].[Lines] L
	JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
	JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
	JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]
	WHERE LD.[LastLineState] = 2
	
	INSERT INTO @Entries (
		[Index], [LineIndex], [DocumentIndex], [Id],
		[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
		[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
		[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
		[NotedAmount], [NotedDate])
	SELECT
		E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
		E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
		E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
		E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
		E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	IF EXISTS(SELECT * FROM @Lines)
--	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__Transition_ToState]
		@Documents = @Documents, 
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @ToState = 2, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 RETURN; -- to avoid NESTED INSERT EXEC

	IF EXISTS(SELECT * FROM @Lines)
	INSERT INTO @ValidationErrors
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, @DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @State = 2,
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 GOTO DONE;

	DELETE FROM @Lines; DELETE FROM @Entries;
	-- Verify that lines whose last state = posted meet the conditions to be posted
	INSERT INTO @Lines(
			[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
			[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
	SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
			L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
	FROM [dbo].[Lines] L
	JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
	JOIN @Ids FE ON FE.[Id] = L.[DocumentId]
	JOIN [map].[Documents]() D ON D.[Id] = FE.[Id]
	WHERE LD.[LastLineState] = 4

	INSERT INTO @Entries (
		[Index], [LineIndex], [DocumentIndex], [Id],
		[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
		[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
		[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
		[NotedAmount], [NotedDate])
	SELECT
		E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
		E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
		E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
		E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
		E.[NotedAmount],E.[NotedDate]
	FROM [dbo].[Entries] E
	JOIN @Lines L ON E.[LineId] = L.[Id];

	IF EXISTS(SELECT * FROM @Lines)
--	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__Transition_ToState]
		@Documents = @Documents, 
		@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @ToState = 4, 
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 RETURN; -- to avoid NESTED INSERT EXEC

	IF EXISTS(SELECT * FROM @Lines)
	INSERT INTO @ValidationErrors -- to avoid NESTED INSERT EXEC
	EXEC [bll].[Lines_Validate__State_Data]
		@Documents = @Documents, @DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries,
		@Lines = @Lines, @Entries = @Entries, @State = 4,
		@Top = @Top, 
		@IsError = @IsError OUTPUT;
	IF @IsError = 1 GOTO DONE;

	DECLARE @CloseValidateScript NVARCHAR (MAX) = (SELECT [CloseValidateScript] FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId);
	IF @CloseValidateScript IS NOT NULL
	BEGIN TRY
		DELETE FROM @Lines; DELETE FROM @Entries;
		-- Pass @Lines and @Entries to the vlidate script
		INSERT INTO @Lines(
				[Index],	[DocumentIndex],[Id],	[DefinitionId], [PostingDate],	[Memo],
				[Decimal1], [Decimal2], [Boolean1], [Text1], [Text2])
		SELECT	L.[Index],	FE.[Index],	L.[Id], L.[DefinitionId], L.[PostingDate], L.[Memo],
				L.[Decimal1], L.[Decimal2], L.[Boolean1], L.[Text1], L.[Text2]
		FROM [dbo].[Lines] L
		JOIN map.LineDefinitions() LD ON LD.[Id] = L.[DefinitionId]
		JOIN @Ids FE ON L.[DocumentId] = FE.[Id]
		JOIN [map].[Documents]() D ON FE.[Id] = D.[Id]

		INSERT INTO @Entries (
			[Index], [LineIndex], [DocumentIndex], [Id],
			[Direction], [AccountId], [CurrencyId], [AgentId], [NotedAgentId], [ResourceId], [NotedResourceId], [CenterId],
			[EntryTypeId], [MonetaryValue], [Quantity], [UnitId], [Value], [RValue], [PValue], [Time1],
			[Time2], [ExternalReference], [ReferenceSourceId], [InternalReference], [NotedAgentName],
			[NotedAmount], [NotedDate])
		SELECT
			E.[Index],L.[Index],L.[DocumentIndex],E.[Id],
			E.[Direction],E.[AccountId],E.[CurrencyId], E.[AgentId], E.[NotedAgentId],E.[ResourceId],E.[NotedResourceId], E.[CenterId],
			E.[EntryTypeId], E.[MonetaryValue],E.[Quantity],E.[UnitId],E.[Value], E.[RValue], E.[PValue], E.[Time1],
			E.[Time2],E.[ExternalReference], E.[ReferenceSourceId], E.[InternalReference],E.[NotedAgentName],
			E.[NotedAmount],E.[NotedDate]
		FROM [dbo].[Entries] E
		JOIN @Lines L ON E.[LineId] = L.[Id];

		INSERT INTO @ValidationErrors
		EXECUTE	dbo.sp_executesql @CloseValidateScript, N'
			@DefinitionId INT,
			@Documents [dbo].[DocumentList] READONLY,
			@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
			@Lines [dbo].[LineList] READONLY, 
			@Entries [dbo].EntryList READONLY,
			@Top INT,
			@UserId INT', 	@DefinitionId = @DefinitionId, @Documents = @Documents,
			@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries, @Lines = @Lines, @Entries = @Entries, @Top = @Top, @UserId = @UserId;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorNumber INT = 100000 + ERROR_NUMBER();
		DECLARE @ErrorMessage NVARCHAR (255) = ERROR_MESSAGE();
		DECLARE @ErrorState TINYINT = 99;
		THROW @ErrorNumber, @ErrorMessage, @ErrorState;
	END CATCH
DONE:
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;
	SELECT TOP (@Top) * FROM @ValidationErrors;
END;
GO
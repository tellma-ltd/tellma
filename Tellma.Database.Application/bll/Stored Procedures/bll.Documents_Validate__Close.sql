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
	-- present BEFORE the close. The vendor call happens after the close commits, so anything it
	-- would refuse must be refused here instead, while the close can still be rolled back;
	-- otherwise the document is closed, claimed and refused, and has to be fixed and resubmitted.
	--
	-- Every fact below is computed the way dal.MarminAe__GetInvoices computes it, defaults
	-- included, so that what is checked is exactly what would be sent.
	IF (SELECT [MarminAeDocumentType] FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId) IS NOT NULL
	BEGIN
		DECLARE @MarminAeDocumentType NVARCHAR (20), @MarminAeTypeCode NVARCHAR (10);
		SELECT @MarminAeDocumentType = [MarminAeDocumentType], @MarminAeTypeCode = [MarminAeTypeCode]
		FROM dbo.DocumentDefinitions WHERE [Id] = @DefinitionId;
		DECLARE @MarminAeIsCreditNote BIT = IIF(@MarminAeDocumentType = N'SalesCreditNote', 1, 0);

		-- The customer party is reached as NotedAgent -> Agent1, and dal.MarminAe__GetInvoices
		-- reaches it through INNER JOINs. A NULL at either hop would therefore drop the document
		-- from the payload without a word, so both hops are guarded explicitly, as the ZATCA block
		-- above guards the first.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_TheDocumentHasMissingInvoice',
			CAST(NULL AS NVARCHAR (255))
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		WHERE D.[NotedAgentId] IS NULL
		UNION
		SELECT DISTINCT TOP (@Top)
			'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
			N'Error_MarminAeDocumentHasNoCustomer',
			SI.[Name]
		FROM @Ids FE
		JOIN dbo.Documents D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]
		WHERE SI.[Agent1Id] IS NULL;

		-- One row per document that does reach a customer.
		DECLARE @MarminAeDocs TABLE (
			[Index]					INT,
			[Id]					INT,
			[Code]					NVARCHAR (255),
			[NotedAgentId]			INT,
			[CustomerName]			NVARCHAR (255),
			[CustomerEmail]			NVARCHAR (255),
			[TaxId]					NVARCHAR (255),
			[Tin]					NVARCHAR (255),
			[IsPeppolRegistered]	BIT,
			[CountryCode]			NVARCHAR (255),
			[Street]				NVARCHAR (255),
			[City]					NVARCHAR (255),
			[Subentity]				NVARCHAR (255),
			[CurrencyId]			NVARCHAR (255),
			[ProfileExecutionId]	NVARCHAR (255),
			[DiscrepancyResponse]	NVARCHAR (255),
			[PaymentMeansCode]		NVARCHAR (255),
			[SalesInvoiceName]		NVARCHAR (255),
			[PayeeAccount]			NVARCHAR (255)
		);

		INSERT INTO @MarminAeDocs
		SELECT
			FE.[Index],
			D.[Id],
			D.[Code],
			D.[NotedAgentId],
			ISNULL(CG.[Name], CA.[Name]),
			ISNULL(CA.[ContactEmail], CG.[ContactEmail]),
			CUST.[TaxId],
			CASE
				WHEN LEN(CUST.[TaxId]) = 10 AND CUST.[TaxId] LIKE N'1%' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN CUST.[TaxId]
				WHEN LEN(CUST.[TaxId]) = 15 AND CUST.[TaxId] LIKE N'1%03' AND CUST.[TaxId] NOT LIKE N'%[^0-9]%'
					THEN LEFT(CUST.[TaxId], 10)
			END,
			IIF(dal.fn_Lookup__Code(ISNULL(CA.[Lookup4Id], CG.[Lookup4Id])) = N'Y', 1, 0),
			dal.fn_Lookup__Code(CA.[AddressCountryId]),
			CA.[AddressStreet],
			CA.[AddressCity],
			CA.[AddressProvince],
			ISNULL(SI.[CurrencyId], D.[CurrencyId]),
			ISNULL(dal.fn_Lookup__Code(D.[Lookup1Id]),
				IIF(ISNULL(dal.fn_Lookup__Code(CA.[AddressCountryId]), N'AE') = N'AE', N'00000000', N'00000001')),
			dal.fn_Lookup__Code(D.[Lookup2Id]),
			ISNULL(dal.fn_Lookup__Code(SI.[Lookup1Id]), N'30'),
			SI.[Name],
			NULLIF(LTRIM(RTRIM(SI.[BankAccountNumber])), N'')
		FROM @Ids FE
		JOIN [map].[Documents]() D ON D.[Id] = FE.[Id]
		JOIN dbo.Agents SI ON SI.[Id] = D.[NotedAgentId]
		JOIN dbo.Agents CA ON CA.[Id] = SI.[Agent1Id]
		LEFT JOIN dbo.Agents CG ON CG.[Id] = CA.[Agent1Id]
		CROSS APPLY (
			SELECT NULLIF(LTRIM(RTRIM(ISNULL(CA.[TaxIdentificationNumber], CG.[TaxIdentificationNumber]))), N'') AS [TaxId]
		) CUST;

		-- One row per invoice line, exactly as dal.MarminAe__GetInvoices selects them: VAT-payable
		-- entries, not discounts/retentions/prepayment applications, and not on a rejected or
		-- voided workflow line, which is not part of the ledger.
		DECLARE @MarminAeLines TABLE (
			[Index]					INT,
			[DocumentId]			INT,
			[ResourceName]			NVARCHAR (255),
			[Quantity]				DECIMAL (19, 6),
			[Price]					DECIMAL (19, 6),
			[UnitCode]				NVARCHAR (255),
			[TaxCategory]			NVARCHAR (255),
			[HasExemptionReason]	BIT,
			[Vat]					DECIMAL (19, 6)
		);

		INSERT INTO @MarminAeLines
		SELECT
			FE.[Index],
			L.[DocumentId],
			NR.[Name],
			IIF(@MarminAeIsCreditNote = 1, +1, -1) * E.[Direction] * E.[Quantity],
			L.[Decimal1],
			U.[Code],
			ISNULL(LK3.[Code], N'S'),
			IIF(NR.[Lookup4Id] IS NULL, 0, 1),
			ROUND((IIF(@MarminAeIsCreditNote = 1, +1, -1) * E.[Direction] * E.[Quantity])
				* L.[Decimal1] * ISNULL(NR.[VatRate], 0.05), 2)
		FROM @Ids FE
		JOIN [map].[Lines]() L ON L.[DocumentId] = FE.[Id]
		JOIN dbo.Entries E ON E.[LineId] = L.[Id]
		JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
		JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = NR.[DefinitionId]
		LEFT JOIN dbo.Lookups LK3 ON LK3.[Id] = NR.[Lookup3Id]
		LEFT JOIN dbo.Units U ON U.[Id] = E.[UnitId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE AC.[Concept] = N'CurrentValueAddedTaxPayables'
		AND L.[State] >= 0
		AND NOT (NRD.[Code] = N'Discounts' OR NR.[Code] = N'RetentionByCustomer'
			OR NRD.[Code] LIKE N'Prepayments%' AND E.[Direction] = 1);

		--=-=-= Customer =-=-=--

		-- The vendor requires an email on the customer party, and Tellma allows it to be blank.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeCustomerHasNoEmail', [CustomerName]
		FROM @MarminAeDocs WHERE [CustomerEmail] IS NULL;

		-- A UAE customer must carry a tax number (it becomes tin and party_tax_scheme), and so must
		-- a Peppol-registered customer anywhere (it becomes the Peppol endpoint). Either way it must
		-- be the 10-digit TIN or the 15-digit TRN, both starting with 1, the TRN ending in 03.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeCustomerHasNoTaxId', [CustomerName]
		FROM @MarminAeDocs WHERE ([CountryCode] = N'AE' OR [IsPeppolRegistered] = 1) AND [TaxId] IS NULL;

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidTaxId', [CustomerName], [TaxId]
		FROM @MarminAeDocs WHERE ([CountryCode] = N'AE' OR [IsPeppolRegistered] = 1) AND [TaxId] IS NOT NULL AND [Tin] IS NULL;

		-- The vendor requires street, city, subentity, country and country code on every address,
		-- UAE or not; the country name is derived from the country, so the country is what is checked.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeCustomerAddressIncomplete', [CustomerName]
		FROM @MarminAeDocs
		WHERE ISNULL([Street], N'') = N'' OR ISNULL([City], N'') = N'' OR ISNULL([Subentity], N'') = N''
		OR ISNULL([CountryCode], N'') = N'';

		-- For a UAE address the subentity must be an emirate CODE. The vendor's own Schematron
		-- asserts exactly this set, and it is what GET /api/codelist/uae-subdivisions returns.
		-- They are three-letter codes (DXB, AUH, ...), not the two-letter ISO 3166-2:AE subdivisions.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidEmirateCode', ISNULL([Subentity], N'')
		FROM @MarminAeDocs
		WHERE [CountryCode] = N'AE'
		AND ISNULL([Subentity], N'') <> N''
		AND [Subentity] NOT IN (N'AUH', N'DXB', N'SHJ', N'UAQ', N'FUJ', N'AJM', N'RAK');

		--=-=-= Document =-=-=--

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_TheInvoiceHasMissingCurrency', CAST(NULL AS NVARCHAR (255))
		FROM @MarminAeDocs WHERE [CurrencyId] IS NULL;

		-- MarminAeTypeCode is also checked when the definition is saved, but a definition saved
		-- before that rule existed would otherwise only be refused by the vendor.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidTypeCode', CAST(NULL AS NVARCHAR (255))
		FROM @MarminAeDocs
		WHERE (@MarminAeDocumentType = N'SalesInvoice' AND ISNULL(@MarminAeTypeCode, N'') NOT IN (N'380', N'480'))
		OR (@MarminAeDocumentType = N'SalesCreditNote' AND ISNULL(@MarminAeTypeCode, N'') NOT IN (N'381', N'81'));

		-- profile_execution_id is validated by the vendor as ^[0-1]{8}$. The defaults are valid, so
		-- this only fires on a Documents.Lookup1 whose code is not eight binary flags.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidProfileExecutionId', [ProfileExecutionId]
		FROM @MarminAeDocs
		WHERE LEN([ProfileExecutionId]) <> 8 OR [ProfileExecutionId] LIKE N'%[^01]%';

		-- A customer outside the UAE can only be invoiced as an export (position 8). Peppol requires
		-- a tin or TRN for any other buyer at a placeholder endpoint (ibr-135-ae), and the vendor
		-- forbids both on a foreign party. Conversely an export is delivered to the customer's
		-- address, which the vendor requires to be outside the UAE.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeForeignCustomerRequiresExport', [CustomerName]
		FROM @MarminAeDocs
		WHERE ISNULL([CountryCode], N'AE') <> N'AE'
		AND LEN([ProfileExecutionId]) = 8 AND SUBSTRING([ProfileExecutionId], 8, 1) <> N'1';

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeExportRequiresForeignCustomer', [CustomerName]
		FROM @MarminAeDocs
		WHERE [CountryCode] = N'AE'
		AND LEN([ProfileExecutionId]) = 8 AND SUBSTRING([ProfileExecutionId], 8, 1) = N'1';

		-- The vendor requires the buyer's legal registration (type and number) on every
		-- out-of-scope invoice, and Tellma does not model one, so a 480 would always be refused
		-- after the close. The 480 rules below still run, so every problem is reported at once.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAe480NotSupported', [Code]
		FROM @MarminAeDocs
		WHERE @MarminAeTypeCode = N'480';

		-- An out-of-scope invoice (480) cannot be a deemed supply (position 2), a profit margin
		-- scheme supply (position 3) or a summary invoice (position 4) -- Peppol IBR-157-AE.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAe480IncompatibleScenario', [ProfileExecutionId]
		FROM @MarminAeDocs
		WHERE @MarminAeTypeCode = N'480'
		AND (SUBSTRING([ProfileExecutionId], 2, 1) = N'1' OR SUBSTRING([ProfileExecutionId], 3, 1) = N'1'
			OR SUBSTRING([ProfileExecutionId], 4, 1) = N'1');

		-- The payment means code (from the sales-invoice agent's Lookup1, default 30) must be one
		-- the vendor accepts (GET /api/codelist/payment-means-modes). ZATCA's 42 and 48 are not.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidPaymentMeans', [PaymentMeansCode]
		FROM @MarminAeDocs
		WHERE [PaymentMeansCode] NOT IN (N'1', N'10', N'20', N'21', N'30', N'49', N'54', N'55', N'68');

		-- A credit transfer must say where to pay (IBR-192-AE): the sales invoice's bank account.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeMissingPayeeAccount', [SalesInvoiceName]
		FROM @MarminAeDocs
		WHERE [PaymentMeansCode] = N'30' AND [PayeeAccount] IS NULL;

		-- A credit note must give its reason as one of the UAE FTA DL8.61.1 codes
		-- (GET /api/codelist/credit-note-reason-codes), held in Documents.Lookup2.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidCreditNoteReason', ISNULL([DiscrepancyResponse], N'')
		FROM @MarminAeDocs
		WHERE @MarminAeIsCreditNote = 1
		AND ISNULL([DiscrepancyResponse], N'') NOT IN (N'DL8.61.1.A', N'DL8.61.1.B', N'DL8.61.1.C', N'DL8.61.1.D', N'DL8.61.1.E', N'VD');

		-- A credit note must name exactly one original invoice. Zero leaves billing_reference
		-- empty, which the vendor rejects; more than one means the NotedAgentId heuristic cannot
		-- tell which invoice is being adjusted. An original closed before the definition became
		-- Marmin-typed (NULL state) counts: the UAE does not require it to have been an e-invoice,
		-- and NULL can only mean that, since a Marmin-typed close always stamps a state. A rejected
		-- invoice (-30) counts too, because the vendor's remedy for one is a credit note. Invoices
		-- refused by the vendor (-10), failing validation (-20) or never sent (0) are not on the
		-- network, and are fixed and resubmitted rather than credited.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST(MD.[Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeNoOriginalInvoice', MD.[Code]
		FROM @MarminAeDocs MD
		WHERE @MarminAeIsCreditNote = 1
		AND (
			SELECT COUNT(*)
			FROM dbo.Documents O
			JOIN dbo.DocumentDefinitions ODD ON ODD.[Id] = O.[DefinitionId]
			WHERE ODD.[MarminAeDocumentType] = N'SalesInvoice'
			AND O.[State] = 1
			AND (O.[MarminAeState] IS NULL OR O.[MarminAeState] >= 1 OR O.[MarminAeState] = -30)
			AND O.[NotedAgentId] = MD.[NotedAgentId]
		) <> 1;

		--=-=-= Lines =-=-=--

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST(MD.[Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeNoInvoiceLines', MD.[Code]
		FROM @MarminAeDocs MD
		WHERE NOT EXISTS (SELECT * FROM @MarminAeLines ML WHERE ML.[DocumentId] = MD.[Id]);

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidQuantity', [ResourceName]
		FROM @MarminAeLines WHERE ISNULL([Quantity], 0) <= 0;

		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeMissingPrice', [ResourceName]
		FROM @MarminAeLines WHERE [Price] IS NULL;

		-- unit_code must be a UN/ECE Recommendation 20 code. It is free text in Tellma, so the most
		-- we can assert here is that there is one at all.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeInvalidUnitCode', [ResourceName]
		FROM @MarminAeLines WHERE ISNULL([UnitCode], N'') = N'';

		-- The vendor requires an exemption reason on every exempt line (Resources.Lookup4).
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeMissingExemptionReason', [ResourceName]
		FROM @MarminAeLines WHERE [TaxCategory] = N'E' AND [HasExemptionReason] = 0;

		-- A tax invoice (380) cannot consist only of exempt (E) or out-of-scope (O) lines. The
		-- vendor refuses it ("Commercial invoice cannot contain only exempt or out of scope
		-- lines"); a zero-rated (Z) line is enough, which is what makes a zero-rated export a 380.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST(MD.[Index] AS NVARCHAR (255)) + ']', N'Error_MarminAe380RequiresTaxableLine', MD.[Code]
		FROM @MarminAeDocs MD
		WHERE @MarminAeTypeCode = N'380'
		AND EXISTS (SELECT * FROM @MarminAeLines ML WHERE ML.[DocumentId] = MD.[Id])
		AND NOT EXISTS (SELECT * FROM @MarminAeLines ML WHERE ML.[DocumentId] = MD.[Id] AND ML.[TaxCategory] NOT IN (N'E', N'O'));

		-- An out-of-scope invoice (480) may only have E, Z or O lines -- Peppol IBR-122-AE.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST([Index] AS NVARCHAR (255)) + ']', N'Error_MarminAe480RequiresOutOfScopeLines', [ResourceName]
		FROM @MarminAeLines
		WHERE @MarminAeTypeCode = N'480'
		AND [TaxCategory] NOT IN (N'E', N'Z', N'O');

		-- The reconciliation check, and the highest-value assertion here.
		--
		-- Marmin computes every total server-side from the lines we send, so a line-mapping error
		-- does not fail loudly: it produces a legally transmitted invoice whose total differs from
		-- the ledger, discovered later by the customer's counterparty. Recomputing the VAT exactly
		-- the way dal.MarminAe__GetInvoices will emit it, and comparing against what the ledger
		-- says, catches that here -- while the close can still be refused.
		--
		-- The ledger side is the logic of dal.fn_Document__InvoiceTotalVatAmountInAccountingCurrency
		-- inlined, with one addition: rejected and voided workflow lines are excluded, as they are
		-- from the payload. The shared function is left alone because ZATCA's BT-111 uses it.
		--
		-- The tolerance absorbs per-line rounding only; a real mapping error (an unmodelled
		-- discount, a missed line) is far larger than a cent.
		INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
		SELECT DISTINCT TOP (@Top) '[' + CAST(MD.[Index] AS NVARCHAR (255)) + ']', N'Error_MarminAeTotalsMismatch', MD.[Code]
		FROM @MarminAeDocs MD
		CROSS APPLY (
			SELECT SUM(ML.[Vat]) AS [Vat] FROM @MarminAeLines ML WHERE ML.[DocumentId] = MD.[Id]
		) MAPPED
		CROSS APPLY (
			SELECT ISNULL(-SUM(E.[Direction] * E.[Value]), 0) AS [Vat]
			FROM dbo.Entries E
			JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
			JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
			JOIN dbo.Lines L ON L.[Id] = E.[LineId]
			JOIN dbo.Resources NR ON NR.[Id] = E.[NotedResourceId]
			WHERE L.[DocumentId] = MD.[Id]
			AND AC.[Concept] = N'CurrentValueAddedTaxPayables'
			AND NR.[Code] NOT LIKE N'Prepayment%'
			AND L.[State] >= 0
		) LEDGER
		WHERE ABS(ISNULL(MAPPED.[Vat], 0) - ABS(LEDGER.[Vat])) > 0.02;
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
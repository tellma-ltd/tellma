CREATE PROCEDURE [bll].[PurchaseInvoice__Fill]
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[DocumentLineList] READONLY, 
	@Entries [dbo].[DocumentLineEntryList] READONLY
AS
SET NOCOUNT ON;
DECLARE @FilledEntries [dbo].[DocumentLineEntryList];
DECLARE @FilledLines dbo.[DocumentLineList];
DECLARE @FunctionalCurrencyId NCHAR(3) = CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId'));

INSERT INTO @FilledEntries SELECT * FROM @Entries;
INSERT INTO @FilledLines SELECT * FROM @Lines;

-- Applies to PurchaseInvoice
-- Set default accounts
DECLARE @VatAccountId INT = (Select [AccountId] FROM dbo.TaxAccounts WHERE [AccountId] = N'VATPurchase'); -- VAT Purchase
UPDATE E
SET AccountId = @VatAccountId
FROM @FilledEntries E JOIN @FilledLines L ON E.DocumentLineIndex = L.[Index]
WHERE L.LineDefinitionId = N'PurchaseInvoice'

-- Applies to All line types
-- Copy information from documents to Lines
--UPDATE L
--SET L.AgentId = D.AgentId
--FROM @FilledLines L JOIN @Documents D ON L.DocumentIndex = D.[Index]
--WHERE 

-- Copy information from Lines to Entries
UPDATE E 
SET E.CurrencyId = L.CurrencyId
FROM @FilledEntries E JOIN @FilledLines L ON E.DocumentLineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.LineDefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.CurrencySource = 2 AND E.CurrencyId = L.CurrencyId

UPDATE E 
SET E.AgentId = L.AgentId
FROM @FilledEntries E JOIN @FilledLines L ON E.DocumentLineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.LineDefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.AgentSource = 2 AND E.AgentId = L.AgentId

UPDATE E 
SET E.ResourceId = L.ResourceId
FROM @FilledEntries E JOIN @FilledLines L ON E.DocumentLineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.LineDefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
WHERE LDE.ResourceSource = 2 AND E.ResourceId = L.ResourceId

-- Copy information from Account to entries
UPDATE E 
SET
	E.AgentId = CASE WHEN A.HasSingleAgent = 1 THEN A.AgentId ELSE E.AgentId END,
	E.ResourceId = CASE WHEN A.HasSingleResource = 1 THEN A.ResourceId ELSE E.ResourceId END,
	E.EntryTypeId = CASE WHEN A.HasSingleEntryTypeId = 1 THEN A.EntryTypeId ELSE E.EntryTypeId END
FROM @FilledEntries E JOIN @FilledLines L ON E.DocumentLineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.LineDefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
JOIN dbo.Accounts A ON E.AccountId = A.Id
WHERE LDE.AccountSource = 1; -- Entered by user

-- Copy information from Resource to entries
-- TODO: Copy all the relevant fixed measures of the resource, as well as other properties like ExternalReference etc..
UPDATE E 
SET
	E.[MonetaryValue] = COALESCE(R.[MonetaryValue], E.[MonetaryValue]),
	E.[Count]		=	COALESCE(R.[Count], E.[Count]),
	E.[Mass]		=	COALESCE(R.[Mass], E.[Mass]),
	E.[Volume]		=	COALESCE(R.[Volume], E.[Volume])
FROM @FilledEntries E JOIN @FilledLines L ON E.DocumentLineIndex = L.[Index]
JOIN dbo.LineDefinitionEntries LDE ON L.LineDefinitionId = LDE.LineDefinitionId AND E.EntryNumber = LDE.EntryNumber
JOIN dbo.Resources R ON E.ResourceId = R.Id
WHERE LDE.ResourceSource = 1; -- Entered by user
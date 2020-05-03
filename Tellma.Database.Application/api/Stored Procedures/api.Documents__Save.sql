CREATE PROCEDURE [api].[Documents__Save]
	@DefinitionId INT,
	@Documents [dbo].[DocumentList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @PreprocessedEntriesJson NVARCHAR (MAX), @PreprocessedEntries dbo.EntryList;

--	INSERT INTO @PreprocessedEntries
	EXEC bll.[Documents__Preprocess]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @Lines,
		@Entries = @Entries,
		@PreprocessedEntriesJson = @PreprocessedEntriesJson OUTPUT;
	--PRINT N'api.Documents__Save: PreprocessedEntriesJson = ' + ISNULL(@PreprocessedEntriesJson, N'');
	
	INSERT INTO @PreprocessedEntries
	SELECT * FROM OpenJson(@PreprocessedEntriesJson)
	WITH (
	[Index]						INT '$.Index',
	[LineIndex]					INT '$.LineIndex',
	[DocumentIndex]				INT '$.DocumentIndex',
	[Id]						INT '$.Id',
	[Direction]					SMALLINT '$.Direction',
	[AccountId]					INT '$.AccountId',
	[CurrencyId]				NCHAR (3) '$.CurrencyId',
	[ContractId]				INT '$.ContractId',
	[ResourceId]				INT '$.ResourceId',
	[CenterId]					INT '$.CenterId',
	[EntryTypeId]				INT '$.EntryTypeId',
	--[BatchCode]					NVARCHAR (50) '$.BatchCode',
	[DueDate]					DATE '$.DueDate',
	[MonetaryValue]				DECIMAL (19,4) '$.MonetaryValue',
	[Quantity]					DECIMAL (19,4) '$.Quantity',
	[UnitId]					INT '$.UnitId',
	[Value]						DECIMAL (19,4) '$.Value',

	[Time1]						DATETIME2 (2) '$.Time1',	-- from time
	[Time2]						DATETIME2 (2) '$.Time2',	-- to time
	[ExternalReference]			NVARCHAR (50) '$.ExternalReference',
	[AdditionalReference]		NVARCHAR (50) '$.AdditionalReference',
	[NotedContractId]				INT '$.NotedContractId',
	[NotedAgentName]			NVARCHAR (50) '$.NotedAgentName',
	[NotedAmount]				DECIMAL (19,4) '$.NotedAmount', 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate]					DATE '$.NotedDate'
	)
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @Lines, -- <== TODO: make it @PreprocessedLines
		@Entries = @PreprocessedEntries;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
	

	EXEC [dal].[Documents__SaveAndRefresh]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @Lines, -- <== TODO: make it @PreprocessedLines
		@Entries = @PreprocessedEntries,
		@ReturnIds = 0;
END;
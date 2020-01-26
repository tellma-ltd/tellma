CREATE PROCEDURE [api].[Documents__Save]
	@DefinitionId NVARCHAR(255),
	@Documents [dbo].[DocumentList] READONLY,
	@WideLines dbo.[WideLineList] READONLY,
	@Lines [dbo].[LineList] READONLY, 
	@Entries [dbo].EntryList READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	DECLARE @AllLines dbo.[LineList];
	DECLARE @AllEntries dbo.EntryList;
	DECLARE @FilledAllEntries [dbo].EntryList;

		
		

	INSERT INTO @AllLines([Index], [DocumentIndex], [Id], [DefinitionId], [CurrencyId], [AgentId], [ResourceId], [MonetaryValue], [Count], [Mass], [Volume], [Time], [Value], [Memo])
	SELECT [Index], [DocumentIndex], [Id], [DefinitionId], [CurrencyId], [AgentId], [ResourceId], [MonetaryValue], [Count], [Mass], [Volume], [Time], [Value], [Memo]
	FROM @Lines
	UNION
	SELECT [Index], [DocumentIndex], [Id], [DefinitionId], [CurrencyId], [AgentId], [ResourceId], [MonetaryValue], [Count], [Mass], [Volume], [Time], [Value], [Memo]
	FROM @WideLines

	INSERT INTO @AllEntries SELECT * FROM @Entries;
	INSERT INTO @AllEntries
	EXEC [bll].[WideLines__Unpivot] @WideLines;

	-- using line definition Id, the entries wil be filled
	INSERT INTO @FilledAllEntries
	EXEC bll.[Documents__Preprocess]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @AllEntries;
			
	--select * from @AllLines;
	--select * from @FilledAllEntries;
	INSERT INTO @ValidationErrors
	EXEC [bll].[Documents_Validate__Save]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @FilledAllEntries;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	DECLARE @DocumentsIndexedIds [dbo].[IndexedIdList];
	INSERT INTO @DocumentsIndexedIds
	EXEC [dal].[Documents__Save]
		@DefinitionId = @DefinitionId,
		@Documents = @Documents,
		@Lines = @AllLines,
		@Entries = @FilledAllEntries,
		@ReturnIds = 1;

	---- Assign the new ones to self
	DECLARE @NewDocumentsIds dbo.IdList;
	INSERT INTO @NewDocumentsIds([Id])
	SELECT Id FROM @DocumentsIndexedIds
	WHERE [Index] IN (SELECT [Index] FROM @Documents WHERE [Id] = 0);

	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	EXEC [dal].[Documents__Assign]
		@Ids = @NewDocumentsIds,
		@AssigneeId = @UserId,
		@Comment = N'FYC'

	IF @ReturnIds = 1
		SELECT * FROM @DocumentsIndexedIds;
END;
CREATE PROCEDURE [api].[Documents__Close]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50) = N'en',
	@NeutralCulture NVARCHAR(50) = N'en',
    @PreviousInvoiceSerialNumber INT OUTPUT,
    @PreviousInvoiceHash NVARCHAR(MAX) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;
	
	-- (1) Validate
	DECLARE @IsError BIT;

	DECLARE @ManualLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ManualLine');
-- Clean residuals resulting from entries cloning
-- May be better handled by C# 
	IF EXISTS (
		SELECT * FROM dbo.[Lines]
		WHERE DefinitionId = @ManualLineDefinitionId
		AND [DocumentId] IN (SELECT [Id] FROM @Ids)
	)
	BEGIN

		UPDATE dbo.Entries
		SET ResourceId = NULL
		WHERE [AccountId] IN
		(	
			SELECT [Id]
			FROM dbo.Accounts A
			WHERE A.AccountTypeId NOT IN (
				SELECT AccountTypeId FROM dbo.AccountTypeResourceDefinitions
			) 
		)
		AND LineId IN (
			SELECT [Id] FROM dbo.Lines 
			WHERE DefinitionId = @ManualLineDefinitionId
			AND [DocumentId] IN (SELECT [Id] FROM @Ids)
		)
		AND [ResourceId] IS NOT NULL;

		UPDATE dbo.Entries
		SET NotedResourceId = NULL
		WHERE [AccountId] IN
		(	
			SELECT [Id]
			FROM dbo.Accounts A
			WHERE A.AccountTypeId NOT IN (
				SELECT AccountTypeId FROM dbo.AccountTypeNotedResourceDefinitions
			) 
		)
		AND LineId IN (
			SELECT [Id] FROM dbo.Lines 
			WHERE DefinitionId = @ManualLineDefinitionId
			AND [DocumentId] IN (SELECT [Id] FROM @Ids)
		)
		AND [NotedResourceId] IS NOT NULL;

		UPDATE dbo.Entries
		SET AgentId = NULL
		WHERE [AccountId] IN
		(	
			SELECT [Id]
			FROM dbo.Accounts A
			WHERE A.AccountTypeId NOT IN (
				SELECT AccountTypeId FROM dbo.AccountTypeAgentDefinitions
			) 
		)
		AND LineId IN (
			SELECT [Id] FROM dbo.Lines 
			WHERE DefinitionId = @ManualLineDefinitionId
			AND [DocumentId] IN (SELECT [Id] FROM @Ids)
		)
		AND [AgentId] IS NOT NULL;

		UPDATE dbo.Entries
		SET NotedAgentId = NULL
		WHERE [AccountId] IN
		(	
			SELECT [Id]
			FROM dbo.Accounts A
			WHERE A.AccountTypeId NOT IN (
				SELECT AccountTypeId FROM dbo.AccountTypeNotedAgentDefinitions
			) 
		)
		AND LineId IN (
			SELECT [Id] FROM dbo.Lines 
			WHERE DefinitionId = @ManualLineDefinitionId
			AND [DocumentId] IN (SELECT [Id] FROM @Ids)
		)
		AND [NotedAgentId] IS NOT NULL;
	END
	
	EXEC [bll].[Lines_Validate__Transition_ToDocumentState]
		@Ids = @Ids, --documents
		@ToDocumentState = 1, -- 0: Open, 1:Close
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;
		
	EXEC [bll].[Documents_Validate__Close]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids,
		@UserId = @UserId,
		@Top = @Top,
		@IsError = @IsError OUTPUT;		

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;

	-- (2) Execute
	EXEC [dal].[Documents__Close]
		@DefinitionId = @DefinitionId,
		@Ids = @Ids, 
		@UserId = @UserId,
		@PreviousInvoiceSerialNumber = @PreviousInvoiceSerialNumber OUTPUT,
		@PreviousInvoiceHash = @PreviousInvoiceHash OUTPUT;
END;
GO
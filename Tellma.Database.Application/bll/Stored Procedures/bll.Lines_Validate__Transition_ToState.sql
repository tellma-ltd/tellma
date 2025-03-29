CREATE PROCEDURE [bll].[Lines_Validate__Transition_ToState]
-- @Lines and @Entries are read from the database just before calling.
	@Documents DocumentList READONLY,
	@DocumentLineDefinitionEntries DocumentLineDefinitionEntryList READONLY, -- TODO: Add to signature everywhere
	@Lines LineList READONLY,
	@Entries EntryList READONLY,
	@ToState SMALLINT,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');

-- moved here from validate sign
DECLARE @PreScript NVARCHAR(MAX) = N'
	SET NOCOUNT ON
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	------
	';
	DECLARE @Script NVARCHAR (MAX);
	DECLARE @PostScript NVARCHAR(MAX) = N'
	-----
	SELECT TOP (@Top) * FROM @ValidationErrors;
	';

	DECLARE @SignValidateScriptLineDefinitions [dbo].[StringList], @LineDefinitionId INT;
	DECLARE @LineState SMALLINT, @D DocumentList, @L LineList, @E EntryList;
	INSERT INTO @SignValidateScriptLineDefinitions
	SELECT DISTINCT DefinitionId FROM @Lines
	WHERE DefinitionId IN (
		SELECT [Id] FROM dbo.LineDefinitions
		WHERE [SignValidateScript] IS NOT NULL
	);

	IF EXISTS (SELECT * FROM @SignValidateScriptLineDefinitions)
	BEGIN
		-- run script to validate information
		DECLARE LineDefinition_Cursor CURSOR FOR SELECT [Id] FROM @SignValidateScriptLineDefinitions;  
		OPEN LineDefinition_Cursor  
		FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId; 
		WHILE @@FETCH_STATUS = 0  
		BEGIN 
			SELECT @Script =  @PreScript + ISNULL([SignValidateScript],N'') + @PostScript
			FROM dbo.LineDefinitions WHERE [Id] = @LineDefinitionId;
			DELETE FROM @L; DELETE FROM @E;
			INSERT INTO @L SELECT * FROM @Lines WHERE DefinitionId = @LineDefinitionId
			INSERT INTO @E SELECT E.* FROM @Entries E JOIN @L L ON E.[LineIndex] = L.[Index] AND E.[DocumentIndex] = L.[DocumentIndex]
			BEGIN TRY
				INSERT INTO @ValidationErrors
				EXECUTE	dbo.sp_executesql @Script, N'
					@LineDefinitionId INT,
					@ToState SMALLINT,
					@Documents [dbo].[DocumentList] READONLY,
					@DocumentLineDefinitionEntries [dbo].[DocumentLineDefinitionEntryList] READONLY,
					@Lines [dbo].[LineList] READONLY, 
					@Entries [dbo].EntryList READONLY,
					@Top INT', 	@LineDefinitionId = @LineDefinitionId, @ToState = @ToState, @Documents = @Documents,
					@DocumentLineDefinitionEntries = @DocumentLineDefinitionEntries, @Lines = @L, @Entries = @E, @Top = @Top;
			END TRY
			BEGIN CATCH
				DECLARE @ErrorNumber INT = 100000 + ERROR_NUMBER();
				DECLARE @ErrorMessage NVARCHAR (255) =
					CAST(@LineDefinitionId AS NVARCHAR (50)) + N':::' + ERROR_MESSAGE();
				DECLARE @ErrorState TINYINT = 99;
				THROW @ErrorNumber, @ErrorMessage, @ErrorState;
			END CATCH
			FETCH NEXT FROM LineDefinition_Cursor INTO @LineDefinitionId;
		END
		CLOSE LineDefinition_Cursor
		DEALLOCATE LineDefinition_Cursor
	END
	
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	IF @IsError = 1 -- 
		SELECT TOP(@Top) * FROM @ValidationErrors;
END;
GO
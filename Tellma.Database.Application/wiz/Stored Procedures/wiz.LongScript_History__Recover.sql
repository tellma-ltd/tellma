CREATE PROCEDURE [wiz].[LongScript_History__Recover]
AS
-- This is a sample code.
-- Please replace table and script name
-- @DefinitionId is the Id of LineDefinition, AgentDefinition or ResourceDefinition
-- If you want to restore the last script, @ValidFrom is typically the last one in the history table
DECLARE @DefinitionId INT = 262;
DECLARE @ValidFrom DATETIME2 = '2023-12-03 14:35:00.7483217';
DECLARE @S NVARCHAR (512) = N'NULL', @I INT = 0;
WHILE LEN(@S) > 0
BEGIN
	SELECT @S = SUBSTRING(ValidateScript, @I, 512), @I = @I + 512
	FROM LineDefinitionsHistory where Id = @DefinitionId and ValidFrom = @ValidFrom
	PRINT @S
END

CREATE PROCEDURE [bll].[Documents_Validate__Cancel]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList], @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	-- All lines must be in negative states.
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Lines[' +
			CAST(DL.[Id] AS NVARCHAR (255)) + ']',
		N'Error_State0IsNotNegative',
		dbo.fn_StateId__State(DL.[State])
	FROM @Ids D
	JOIN dbo.[Lines] DL ON DL.[DocumentId] = D.[Id]
	WHERE DL.[State] >= 0

	SELECT TOP (@Top) * FROM @ValidationErrors;
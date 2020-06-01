CREATE PROCEDURE [bll].[Documents_Validate__Assign]
	@Ids [dbo].[IndexedIdList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Must not assign a document that is already posted/canceled
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		CASE
			WHEN D.[State] = 1 THEN N'Error_CannotAssignPostedDocuments'
			WHEN D.[State] = -1 THEN N'Error_CannotAssignCanceledDocuments'
		END
	FROM @Ids FE
	JOIN [dbo].[Documents] D ON FE.[Id] = D.[Id]
	WHERE D.[State] <> 0; -- Posted or Canceled

	-- Must not assign a document to the same assignee
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheDocumentIsAlreadyAssignedToUser0',
		dbo.fn_Localize(U.[Name], U.[Name2], U.[Name3]) As Assignee
	FROM @Ids FE
	JOIN [dbo].[DocumentAssignments] DA ON FE.[Id] = DA.[DocumentId]
	JOIN dbo.Users U ON DA.AssigneeId = U.[Id]
	WHERE DA.AssigneeId = @AssigneeId
			
	SELECT TOP (@Top) * FROM @ValidationErrors;
CREATE PROCEDURE [bll].[AgentRelations_Validate__Save]
	@DefinitionId NVARCHAR (255),
	@Entities [dbo].[AgentRelationList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

    -- Non Null Ids must exist
    INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
    SELECT
		'[' + CAST([Index] AS NVARCHAR (255)) + '].Id',
		N'Error_TheId0WasNotFound',
		CAST([Id] As NVARCHAR (255))
    FROM @Entities
    WHERE Id <> 0
	AND Id NOT IN (SELECT Id from [dbo].[AgentRelations]);

	-- Relation must be unique
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1] )
	SELECT
		'[' + CAST(FE.[Index] AS NVARCHAR (255)) + '].AgentId',
		N'Error_TheRelation0IsAlreadyEstablishedForAgent1',
		@DefinitionId, AG.[Name]
	FROM @Entities FE 
	JOIN [dbo].[AgentRelations] BE ON BE.AgentRelationDefinitionId = @DefinitionId AND BE.AgentId =  FE.AgentId
	JOIN dbo.[Agents] AG ON BE.AgentId = AG.Id
	WHERE (FE.Id <> BE.Id);

	SELECT TOP (@Top) * FROM @ValidationErrors;
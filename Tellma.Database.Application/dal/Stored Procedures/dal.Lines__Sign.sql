CREATE PROCEDURE [dal].[Lines__Sign]
	@Ids dbo.[IdList] READONLY,
	@ToState SMALLINT,
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@OnBehalfOfuserId INT,
	@RuleType NVARCHAR (50),
	@RoleId INT,
	@SignedAt DATETIMEOFFSET(7)
AS
BEGIN
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO dbo.[LineSignatures] (
		[LineId], [ToState], [ReasonId], [ReasonDetails], [OnBehalfOfUserId],			[RuleType], [RoleId], [SignedAt]
	)
	SELECT
		[Id], @ToState,	@ReasonId,	@ReasonDetails,	 ISNULL(@OnBehalfOfuserId, @UserId), @RuleType, @RoleId, @SignedAt
	FROM @Ids

	SELECT DISTINCT [DocumentId] FROM [dbo].[Lines] WHERE [Id] IN (SELECT [Id] FROM @Ids)
END;
CREATE PROCEDURE [dal].[Lines__Sign]
	@Ids dbo.[IdList] READONLY,
	@ToState SMALLINT, -- NVARCHAR(30),
	@ReasonId INT,
	@ReasonDetails	NVARCHAR(1024),
	@OnBehalfOfuserId INT,
	@RoleId INT,
	@SignedAt DATETIMEOFFSET(7)
AS
BEGIN
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
-- TODO: make sure signatures are not repeated if signing two times in a row
	INSERT INTO dbo.[LineSignatures] (
		[LineId], [ToState], [ReasonId], [ReasonDetails], [OnBehalfOfuserId], [RoleId], [SignedAt]
	)
	SELECT
		[Id],	@ToState,	@ReasonId,	@ReasonDetails,	 ISNULL(@OnBehalfOfuserId, @UserId), @RoleId, @SignedAt
	FROM @Ids
END;